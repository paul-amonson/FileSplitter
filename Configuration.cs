///////////////////////////////////////////////////////////////////////
//        Copyright©2003-2022 Paul Amonson, All Rights Reserved      //
///////////////////////////////////////////////////////////////////////
#region Using Section
using System;
using System.IO;
using System.Drawing;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Soap;
using System.Windows.Forms;
#endregion

namespace FileSplitter
{
	[Serializable]
	public class Configuration
	{
		[NonSerialized] private bool _modified;

		private int     _radioIndex;
		private Point   _position;
		private Size    _size;
        private string  _mask;
        private long[]  _fileSizes;
        private int     _typeIndex;
        private bool    _generateHash;

		// Construction
		public Configuration()
		{
			_modified = false;
		}
		
		public void UseDefaults()
		{
            _radioIndex     = 4;
            _mask           = "nnn";
            _size.Width     = 512;
            _size.Height    = 368;
			_position.X     = 0;
			_position.Y     = 0;
            _fileSizes      = new long[7];
            _fileSizes[0]   =    1457664L;
            _fileSizes[1]   =  100431872L;
            _fileSizes[2]   =  250000000L;
            _fileSizes[3]   =  681574400L;
            _fileSizes[4]   =  734003200L;
            _fileSizes[5]   = 2147483647L;
            _fileSizes[6]   =    1048576L;
            _typeIndex      = 2;
            _generateHash   = true;

			_modified = true;
		}

		public void Save()
		{
			string filename = Application.ExecutablePath + ".prefs";
			if(_modified == true)
			{
				IFormatter formatter = new SoapFormatter();
				if(File.Exists(filename) == true)
				{
					File.Delete(filename);
				}
				Stream stream = File.Create(filename);
				formatter.Serialize(stream, this);
				stream.Flush();
				stream.Close();
			}
		}

		static public Configuration Load()
		{
			Configuration config = null;
			
			string filename = Application.ExecutablePath + ".prefs";

			try
			{
				IFormatter formatter = new SoapFormatter();
				Stream stream = File.OpenRead(filename);
				config = (Configuration)formatter.Deserialize(stream);
				stream.Close();
			}
			catch(Exception)
			{
				config = new Configuration();
				config.UseDefaults();
			}

			return config;
		}

		public Point WindowLocation
		{
			get
			{
				return _position;
			}
			set
			{
				_position = value;
				_modified = true;
			}
		}

		public Size WindowSize
		{
			get
			{
				return _size;
			}
			set
			{
				_size = value;
				_modified = true;
			}
		}

		public int SelectedRadioButton 
		{
			get
			{
				return _radioIndex;
			}
			set
			{
				_radioIndex = value;
				_modified = true;
			}
		}

		public string OutputMask
		{
			get
			{
				return _mask;
			}
			set
			{
				_mask = value;
				_modified = true;
			}
		}

		public int CustomType
		{
			get
			{
				return _typeIndex;
			}
            set
            {
                _typeIndex = value;
                _modified = true;
            }
		}

        public long this[int index]
        {
            get
            {
                if(index < 0 || index > 6)
                {
                    throw new ArgumentException("Index out of range.  Must be 0-6 inclusive.", "index");
                }
                return _fileSizes[index];
            }
            set
            {
                if(index < 0 || index > 6)
                {
                    throw new ArgumentException("Index out of range.  Must be 0-6 inclusive.", "index");
                }
                _fileSizes[index] = value;
                _modified = true;
            }
        }

        public bool GenerateHash
        {
            get
            {
                return _generateHash;
            }
            set
            {
                _generateHash = value;
                _modified = true;
            }
        }
	}
}
