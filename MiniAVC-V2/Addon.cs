﻿// Copyright (C) 2014 CYBUTEK
//
// This program is free software: you can redistribute it and/or modify it under the terms of the GNU
// General Public License as published by the Free Software Foundation, either version 3 of the
// License, or (at your option) any later version.
//
// This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without
// even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU
// General Public License for more details.
//
// You should have received a copy of the GNU General Public License along with this program. If not,
// see <http://www.gnu.org/licenses/>.

using System;
using System.IO;
using System.Net;
//using System.Threading;
using UnityEngine;
using UnityEngine.Networking;

namespace MiniAVC_V2
{
    public class Addon
    {
        private readonly AddonSettings settings;

        public Addon(string path, AddonSettings settings)
        {
            this.settings = settings;
            RunProcessLocalInfo(path);
        }

        public string Base64String
        {
            get
            {
                return LocalInfo.Base64String + RemoteInfo.Base64String;
            }
        }

        public bool HasError { get; private set; }

        public bool IsCompatible
        {
            get { return IsLocalReady && LocalInfo.IsCompatible; }
        }

        public bool IsIgnored
        {
            get
            {
                return settings.IgnoredUpdates.Contains(Base64String);
            }
        }

        public bool IsLocalReady { get; private set; }

        public bool IsProcessingComplete { get; private set; }

        public bool IsRemoteReady { get; private set; }

        public bool IsUpdateAvailable
        {
            get
            {
                bool b = this.IsProcessingComplete &&
                  this.LocalInfo.Version != null &&
                  this.RemoteInfo.Version != null &&
                  this.RemoteInfo.Version > this.LocalInfo.Version &&
                  // this.RemoteInfo.IsCompatibleKspVersion && 
                  this.RemoteInfo.IsCompatible &&
                  this.RemoteInfo.IsCompatibleGitHubVersion;

                return b;
            }
        }
        public AddonInfo LocalInfo { get; private set; }

        public string Name
        {
            get { return LocalInfo.Name; }
        }

        public AddonInfo RemoteInfo { get; private set; }

        public AddonSettings Settings
        {
            get { return settings; }
        }

        public void RunProcessLocalInfo(string file)
        {
            ProcessLocalInfo(file);
            //ThreadPool.QueueUserWorkItem(ProcessLocalInfo, file);
        }

        public void RunProcessRemoteInfo()
        {
            ProcessRemoteInfo(null);
            //ThreadPool.QueueUserWorkItem(ProcessRemoteInfo);
        }

        private void FetchLocalInfo(string path)
        {
            using (var stream = new StreamReader(File.OpenRead(path)))
            {
                LocalInfo = new AddonInfo(path, stream.ReadToEnd());
                IsLocalReady = true;

                if (LocalInfo.ParseError)
                {
                    SetHasError();
                }
            }
        }

        private void FetchRemoteInfo()
        {
            try
            {
                HttpWebRequest request = HttpWebRequest.Create(Uri.EscapeUriString(this.LocalInfo.Url)) as HttpWebRequest;
                request.Method = WebRequestMethods.Http.Get;
                request.Timeout = 10000;  // milliseconds
                using (HttpWebResponse response = request.GetResponse() as HttpWebResponse)
                {
                    if (response.StatusCode == HttpStatusCode.OK)
                    {
                        Stream data = response.GetResponseStream();
                        string html = String.Empty;
                        using (StreamReader sr = new StreamReader(data))
                        {
                            html = sr.ReadToEnd();
                        }
                        response.Close();
                        this.SetRemoteInfo(html);
                    }
                    else
                    {
                        SetLocalInfoOnly();
                    }
                }
            }
            catch (WebException ex)
            {
                Logger.Log("Exception fetching data from: " + this.LocalInfo.Url);
                if (ex.Status == WebExceptionStatus.ProtocolError)
                {
                    Logger.Log("Status Code : " + ((int)((HttpWebResponse)ex.Response).StatusCode).ToString() + " - " + ((HttpWebResponse)ex.Response).StatusCode.ToString());
                    Logger.Log("Status Description : " + ((HttpWebResponse)ex.Response).StatusDescription);
                }
                else
                    Logger.Exception(ex);

                this.SetLocalInfoOnly();
            }

#if false
            using (UnityWebRequest www = UnityWebRequest.Get(Uri.EscapeUriString(LocalInfo.Url)))
            {
                while (!www.isDone)
                {
                    Thread.Sleep(100);
                }
                if (www.error == null)
                {
                    SetRemoteInfo(www);
                }
                else
                {
                    SetLocalInfoOnly();
                }
            }
#endif
        }

        private void ProcessLocalInfo(object state)
        {
            try
            {
                var path = (string)state;
                if (File.Exists(path))
                {
                    FetchLocalInfo(path);
                    RunProcessRemoteInfo();
                }
                else
                {
                    Logger.Log("File Not Found: " + path);
                    SetHasError();
                }
            }
            catch (Exception ex)
            {
                Logger.Exception(ex);
                SetHasError();
            }
        }

        private void ProcessRemoteInfo(object state)
        {
            try
            {
                if (settings.FirstRun)
                {
                    return;
                }

                if (!settings.AllowCheck || string.IsNullOrEmpty(LocalInfo.Url))
                {
                    SetLocalInfoOnly();
                    return;
                }

                FetchRemoteInfo();
            }
            catch (Exception ex)
            {
                Logger.Exception(ex);
                SetLocalInfoOnly();
            }
        }

        private void SetHasError()
        {
            HasError = true;
            IsProcessingComplete = true;
        }

        private void SetLocalInfoOnly()
        {
            RemoteInfo = LocalInfo;
            IsRemoteReady = true;
            IsProcessingComplete = true;
            Logger.Log(LocalInfo);
            Logger.Blank();
        }

#if false
        private void SetRemoteInfo(UnityWebRequest www)
        {
            SetRemoteInfo(www.url);
        }
#endif
        private void SetRemoteInfo(string json)
        {
            RemoteInfo = new AddonInfo(LocalInfo.Url, json);
            RemoteInfo.FetchRemoteData();
#if true
            if (LocalInfo.Version == RemoteInfo.Version)
            {
                Logger.Log("Identical remote version found: Using remote version information only.");
                Logger.Log(RemoteInfo);
                Logger.Blank();
                LocalInfo = RemoteInfo;
            }
            else
#endif
            {
                Logger.Log(LocalInfo);
                Logger.Log(RemoteInfo + "\n\tUpdateAvailable: " + IsUpdateAvailable);
                Logger.Blank();
            }

            IsRemoteReady = true;
            IsProcessingComplete = true;
        }
    }
}
