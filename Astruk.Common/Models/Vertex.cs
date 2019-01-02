namespace Astruk.Common.Models
{
	public class Vertex
	{
		public Vertex(int id, double x, double y)
		{
			Id = id;
			X = x;
			Y = y;
		}

		public int Id { get; }
		public double X { get; }
		public double Y { get; }
	}
}
