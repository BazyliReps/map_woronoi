namespace Astruk.Common.Models
{
	public class KeyMapObject
	{
		public KeyMapObject(int id, double x, double y, string name)
		{
			Id = id;
			X = x;
			Y = y;
			Name = name;
		}

		public int Id { get; }
		public double X { get; }
		public double Y { get; }
		public string Name { get; }
	}
}