using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NumratorCameraApp
{
    public class MyAcquisitionParams
    {
        public MyAcquisitionParams()
        {
            m_ServerName = "";
            m_ResourceIndex = 0;
            m_ServerIndex = 0;
            m_GrabberConfigFileName = "";
            m_CameraConfigFileName = "";
        }

        public MyAcquisitionParams(string ServerName,int ServerIndex, int ResourceIndex)
        {
            m_ServerName = ServerName;
            m_ServerIndex = ServerIndex;
            m_ResourceIndex = ResourceIndex;
            m_GrabberConfigFileName = "";
            m_CameraConfigFileName = "";
        }

        public MyAcquisitionParams(string ServerName,int ServerIndex, int ResourceIndex, string GrabberConfigFileName, string CameraConfigFileName)
        {
            m_ServerName = ServerName;
            m_ResourceIndex = ResourceIndex;
            m_ServerIndex = ServerIndex;
            m_GrabberConfigFileName = GrabberConfigFileName;
            m_CameraConfigFileName = CameraConfigFileName;
        }

        public string GrabberConfigFileName
        {
            get { return m_GrabberConfigFileName; }
            set { m_GrabberConfigFileName = value; }
        }
        public string CameraConfigFileName
        {
            get { return m_CameraConfigFileName; }
            set { m_CameraConfigFileName = value; }
        }

        public string ServerName
        {
            get { return m_ServerName; }
            set { m_ServerName = value; }
        }

        public int ResourceIndex
        {
            get { return m_ResourceIndex; }
            set { m_ResourceIndex = value; }
        }
        public int ServerIndex
        {
            get { return m_ServerIndex; }
            set { m_ServerIndex = value; }
        }
        protected string m_ServerName;
        protected int m_ResourceIndex=0;
        protected int m_ServerIndex = 0;
        protected string m_GrabberConfigFileName;
        protected string m_CameraConfigFileName;
    }
}
