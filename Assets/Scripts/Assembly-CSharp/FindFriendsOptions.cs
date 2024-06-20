public class FindFriendsOptions
{
	public bool CreatedOnGs;

	public bool Visible;

	public bool Open;

	internal int ToIntFlags()
	{
		int num = 0;
		if (CreatedOnGs)
		{
			num |= 1;
		}
		if (Visible)
		{
			num |= 2;
		}
		if (Open)
		{
			num |= 4;
		}
		return num;
	}
}
