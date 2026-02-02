using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TrailMeister.Model
{
    public interface ITagReaderConfig
    {
        public void StartReader(int power);

        public void StopReader();

        public void Reset();
    }
}
