using System;
using System.Runtime.InteropServices;
using System.Reflection;
using System.IO;
using System.IO.IsolatedStorage;

namespace ZeroMQ.Interop
{
    public class IsolatedLoader
    {
        private SafeLibraryHandle handle;

        public TDelegate GetUnmanagedFunction<TDelegate>(string functionName) where TDelegate : class
        {
            var addr = Platform.LoadProcedure(handle, functionName);
            return (TDelegate)(object)Marshal.GetDelegateForFunctionPointer(addr, typeof(TDelegate));
        }

        public bool Load(byte[] data)
        {
            var store = IsolatedStorageFile.GetStore(IsolatedStorageScope.User | IsolatedStorageScope.Domain | IsolatedStorageScope.Assembly, null, null);
            var stream = new IsolatedStorageFileStream("x.dll", FileMode.Create, store);
            string path = stream.GetType().GetField("m_FullPath", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(stream).ToString();

            stream.Write(data, 0, data.Length);
            stream.Flush();
            stream.Close();

            handle = Platform.OpenHandle(path);

            return true;
        }

        public bool LoadResource(string resourceName)
        {
            using (var resourceStream = Assembly.GetExecutingAssembly().GetManifestResourceStream(resourceName))
            {
                if (resourceStream == null)
                {
                    // No manifest resources were compiled into the current assembly. This is likely a 'manual
                    // deployment' situation, so do not throw an exception at this point and allow all deployment
                    // paths to be searched.
                    return false;
                }
                int len = (int)resourceStream.Length;
                var bytes = new byte[len];
                resourceStream.Read(bytes, 0, len);
                return Load(bytes);
            }
        }
    }
}