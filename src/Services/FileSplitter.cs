using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services
{
    public interface IFileSplitter
    {
        void GetRawGamesFromPgnFile(string pgnFile);
    }

    public class FileSplitter : IFileSplitter
    {
        public void GetRawGamesFromPgnFile(string pgnFile)
        {
            throw new NotImplementedException();
        }
    }
}
