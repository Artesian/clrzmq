﻿using System;
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
            var store = IsolatedStorageFile.GetStore(IsolatedStorageScope.Machine, null, null);
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
                    throw new Exception(string.Format("Couldn't find {0} in assembly resources", resourceName));
                }
                int len = (int)resourceStream.Length;
                var bytes = new byte[len];
                resourceStream.Read(bytes, 0, len);
                return Load(bytes);
            }
        }
    }
}