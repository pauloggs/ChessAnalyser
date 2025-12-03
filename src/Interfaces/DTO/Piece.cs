using System;
namespace Interfaces.DTO
{
	public class Piece
	{
		//public char Name { get; set; } = 'P';
		//public double Value { get; set; }

        public char Name { get; }
        public double Value { get; }

        public Piece(char name, double value)
        {
            Name = name;
            Value = value;
        }
    }
}

