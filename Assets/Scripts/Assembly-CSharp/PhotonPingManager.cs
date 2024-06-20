using System;
using System.Collections;
using System.Diagnostics;
using System.Net;
using Photon.Realtime;
using UnityEngine;

public class PhotonPingManager
{
	public bool UseNative;

	public static int Attempts = 5;

	public static bool IgnoreInitialAttempt = true;

	public static int MaxMilliseconsPerPing = 800;

	private const string wssProtocolString = "wss://";

	private int PingsRunning;

	public Region BestRegion
	{
		get
		{
			Region result = null;
			int num = int.MaxValue;
			foreach (Region availableRegion in PhotonNetwork.networkingPeer.AvailableRegions)
			{
				UnityEngine.Debug.Log("BestRegion checks region: " + availableRegion);
				if (availableRegion.Ping != 0 && availableRegion.Ping < num)
				{
					num = availableRegion.Ping;
					result = availableRegion;
				}
			}
			return result;
		}
	}

	public bool Done => PingsRunning == 0;

	public IEnumerator PingSocket(Region region)
	{
		region.Ping = Attempts * MaxMilliseconsPerPing;
		PingsRunning++;
		PhotonPing ping = null;
		if (PhotonHandler.PingImplementation == typeof(PingMono))
		{
			ping = new PingMono();
		}
		if (ping == null)
		{
			ping = (PhotonPing)Activator.CreateInstance(PhotonHandler.PingImplementation);
		}
		float rttSum = 0f;
		int replyCount = 0;
		string regionAddress2 = region.HostAndPort;
		int num = regionAddress2.LastIndexOf(':');
		if (num > 1)
		{
			regionAddress2 = regionAddress2.Substring(0, num);
		}
		int num2 = regionAddress2.IndexOf("wss://");
		if (num2 > -1)
		{
			regionAddress2 = regionAddress2.Substring(num2 + "wss://".Length);
		}
		regionAddress2 = ResolveHost(regionAddress2);
		for (int i = 0; i < Attempts; i++)
		{
			bool overtime = false;
			Stopwatch sw = new Stopwatch();
			sw.Start();
			try
			{
				ping.StartPing(regionAddress2);
			}
			catch (Exception ex)
			{
				UnityEngine.Debug.Log("catched: " + ex);
				PingsRunning--;
				break;
			}
			while (!ping.Done())
			{
				if (sw.ElapsedMilliseconds >= MaxMilliseconsPerPing)
				{
					overtime = true;
					break;
				}
				yield return 0;
			}
			int num3 = (int)sw.ElapsedMilliseconds;
			if ((!IgnoreInitialAttempt || i != 0) && ping.Successful && !overtime)
			{
				rttSum += (float)num3;
				replyCount++;
				region.Ping = (int)(rttSum / (float)replyCount);
			}
			yield return new WaitForSeconds(0.1f);
		}
		ping.Dispose();
		PingsRunning--;
		yield return null;
	}

	public static string ResolveHost(string hostName)
	{
		string text = string.Empty;
		try
		{
			IPAddress[] hostAddresses = Dns.GetHostAddresses(hostName);
			if (hostAddresses.Length == 1)
			{
				return hostAddresses[0].ToString();
			}
			foreach (IPAddress iPAddress in hostAddresses)
			{
				if (iPAddress != null)
				{
					if (iPAddress.ToString().Contains(":"))
					{
						return iPAddress.ToString();
					}
					if (string.IsNullOrEmpty(text))
					{
						text = hostAddresses.ToString();
					}
				}
			}
		}
		catch (Exception ex)
		{
			UnityEngine.Debug.Log("Exception caught! " + ex.Source + " Message: " + ex.Message);
		}
		return text;
	}
}
