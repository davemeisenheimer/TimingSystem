using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TrailMeister.Model.Arduino;
using TrailMeister.Model.M6ENano;
using TrailMeisterUtilities;

namespace TrailMeister.Model
{
    public class TagReader: Disposable, ITagDataSource
    {

        private ITagDataSource _dataSource;

        public TagReader(TagReaderDataSourceType dataSource)
        {
            switch(dataSource) {
                case TagReaderDataSourceType.Arduino:
                    _dataSource = ArduinoDataSource.Instance;
                    break;
                case TagReaderDataSourceType.M6ENano:
                default:
                    _dataSource = new M6ENanoDataSource();
                    break;
            }
        }

        public void init()
        {
            _dataSource.init();
        }

        public event TagDataSourceEventHandler TagDataSourceEvent
        {
            add
            {
                _dataSource.TagDataSourceEvent += value;
            }

            remove
            {
                _dataSource.TagDataSourceEvent -= value;
            }
        }

        public ITagReaderConfig Config { get { return _dataSource.Config; } }

        TagReaderDataSourceType ITagDataSource.DataSourceType
        {
            get { return _dataSource.DataSourceType; }
        }

        protected override void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    //dispose managed resources
                }
            }
            //dispose unmanaged resources
            _dataSource.Dispose();

            _disposed = true;
        }
    }
}
