/*
Copyright 2015-2017 Daniel Adrian Redondo Suarez

Licensed under the Apache License, Version 2.0 (the "License");
you may not use this file except in compliance with the License.
You may obtain a copy of the License at

    http://www.apache.org/licenses/LICENSE-2.0

Unless required by applicable law or agreed to in writing, software
distributed under the License is distributed on an "AS IS" BASIS,
WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
See the License for the specific language governing permissions and
limitations under the License.
 */

using System;
using System.Collections.Concurrent;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;

namespace TWCore.Diagnostics.Log.Storages
{
    /// <summary>
    /// Writes an html log file
    /// </summary>
	public class HtmlFileLogStorage : ILogStorage
    {
        static ConcurrentDictionary<string, StreamWriter> logStreams = new ConcurrentDictionary<string, StreamWriter>();
        StreamWriter sWriter;
        string currentFileName;
        int numbersOfFiles;
        volatile bool firstWrite = true;

		#region Html format
		string _html = @"
<html>
<head>
    <title>Log File</title>
	<script src='https://code.jquery.com/jquery-3.2.1.slim.min.js' integrity='sha256-k2WSCIexGzOj3Euiig+TlR8gA0EmPjuc79OEeY5L45g=' crossorigin='anonymous'></script>
    <style>
        html { background-color: #000; }
        body { background-color: #000 }
        .displaywrap::-webkit-scrollbar-track { border-radius: 0px; background-color: #181818; }
        .displaywrap::-webkit-scrollbar { width: 18px; height: 18px; background-color: #000; }
        .displaywrap::-webkit-scrollbar-thumb { border-radius: 2px; -webkit-box-shadow: inset 0 0 6px rgba(0,0,0,.3); background-color: #333; }
        .displaywrap {
            position: absolute;
            overflow: overlay;
            top: 0px; bottom: 0px; left: 0px; right: 0px;
            border: 0px solid #333; border-radius: 0px;
            background: #000; color: #fff;
            font-family: Consolas, monospace; font-size: 0.8em;
            padding: 40px 0px 20px 0px;
        }
        .menu {
            position: absolute;
            top: 0px; height: 27px; left: 0px; right: 18px;
            background: rgba(25,25,25,0.95);
            border-bottom: 1px solid #0c539d;
            padding: 5px;
            z-index: 100;
            overflow: hidden;
        }
        .switch {
            position: relative;
            display: inline-block;
            width: 45px;
            height: 28px;
            margin-right: 10px;
        }
        .text {
            position: relative;
            display: inline-block;
            height: 28px;
            width: 80px;
        }
        .text div {
            position: absolute;
            top: 0px; left: 0px; height: 28px; width: 100%;
            padding: 0px;
            color: #ccc;
            font-family: 'Trebuchet MS', 'Lucida Sans Unicode', 'Lucida Grande', 'Lucida Sans', Arial, sans-serif;
            font-size: 0.75em;
            text-align:center;
        }
        .slider {
            position: absolute;
            cursor: pointer;
            top: 0;
            left: 0;
            right: 0;
            bottom: 0;
            background-color: #ccc;
            -webkit-transition: .4s;
            transition: .4s;
        }
        .slider:before {
            position: absolute;
            content: '';
            height: 20px;
            width: 20px;
            left: 4px;
            bottom: 4px;
            background-color: white;
            -webkit-transition: .4s;
            transition: .4s;
        }
        input:checked + .slider { background-color: #0351a0; }
        input:focus + .slider { box-shadow: 0 0 1px #0351a0; }
        input:checked + .slider:before {
            -webkit-transform: translateX(16px);
            -ms-transform: translateX(16px);
            transform: translateX(16px);
        }
        .slider.round { border-radius: 34px; }
        .slider.round:before { border-radius: 50%; }
        .switch input {display:none;}
        .display { padding: 5px; }
        #dvStarts {
            position: absolute;
            background-color: #222;
            top: 45px;
            right: 25px;
            z-index: 1000;
            opacity: 0.8;
            text-align: center;
            padding: 10px;
            color: white;
            font-family: monospace;
            overflow-y: auto;
            border-radius: 5px;
            font-size: 0.86em;
        }
        #ulStarts {
            list-style-type: none;
            margin: 0px;
            padding: 5px;
            line-height: 1.5em;
        }
        #ulStarts li {
            cursor: pointer;
        }
        .liActive {
            color: #1b8aff;
            font-weight: bolder;
            font-size: 1.2em;
        }
        pre { padding:0px; margin: 0px; font-family: Consolas, monospace; }
        .Start { color: brown; font-size:1.3em; }
        .End { color: brown; font-size:1.3em; }
        .Error { color: red; }
        .Warning { color: yellow; }
        .InfoBasic { color: cyan; }
        .InfoMedium { color: cyan; }
        .InfoDetail { color: cyan; }
        .Debug { color: darkcyan; }
        .LibDebug { color: darkcyan; }
        .Stats { color: darkgreen; }
        .Verbose { color: gray; }
        .LibVerbose { color: darkgray; }
    </style>
</head>
<body>
<div id='dvStarts'>STARTS<ul id='ulStarts'></ul></div>
<div class='menu'>
    <label class='text' style='width:65px;'><div id='dvError'>Error</div></label>
    <label class='switch'><input type='checkbox' id='chkError' checked><span class='slider round'></span></label>
    <label class='text'><div id='dvWarning'>Warning</div></label>
    <label class='switch'><input type='checkbox' id='chkWarning' checked><span class='slider round'></span></label>
    <label class='text' style='width:70px;'><div id='dvInfoB'>Info B</div></label>
    <label class='switch'><input type='checkbox' id='chkInfoB' checked><span class='slider round'></span></label>
    <label class='text' style='width:70px;'><div id='dvInfoM'>Info M</div></label>
    <label class='switch'><input type='checkbox' id='chkInfoM' checked><span class='slider round'></span></label>
    <label class='text' style='width:70px;'><div id='dvInfoD'>Info D</div></label>
    <label class='switch'><input type='checkbox' id='chkInfoD' checked><span class='slider round'></span></label>
    <label class='text' style='width:70px;'><div id='dvDebug'>Debug</div></label>
    <label class='switch'><input type='checkbox' id='chkDebug' checked><span class='slider round'></span></label>
    <label class='text' style='width:70px;'><div id='dvStats'>Stats</div></label>
    <label class='switch'><input type='checkbox' id='chkStats' checked><span class='slider round'></span></label>
    <label class='text'><div id='dvVerbose'>Verbose</div></label>
    <label class='switch'><input type='checkbox' id='chkVerbose' checked><span class='slider round'></span></label>
    <label class='text' style='width:55px;'><div>Empty<br/>Lines</div></label>
    <label class='switch'><input type='checkbox' id='chkEmptyLines' checked><span class='slider round'></span></label>
    <script>
        var starts = { 
            data : [],
            goTo : function(index) { $('.displaywrap').scrollTop(this.data[index][1].offset().top - this.data[index][1].parent().position().top); },
            goToEnd : function() {$('.displaywrap').scrollTop($('.display').height()); }
        };
        $(function() {
            $('#dvError').html('Error<br/>' + $('.Error').length);
            $('#dvWarning').html('Warning<br/>' + $('.Warning').length);
            $('#dvInfoB').html('Info B<br/>' + $('.InfoBasic').length);
            $('#dvInfoM').html('Info M<br/>' + $('.InfoMedium').length);
            $('#dvInfoD').html('Info D<br/>' + $('.InfoDetail').length);
            $('#dvDebug').html('Debug<br/>' + ($('.Debug').length + $('.LibDebug').length));
            $('#dvStats').html('Stats<br/>' + $('.Stats').length);
            $('#dvVerbose').html('Verbose<br/>' + ($('.Verbose').length + $('.LibVerbose').length));

            if ($('.Error').length == 0) $('#chkError').parent().remove();
            if ($('.Warning').length == 0) $('#chkWarning').parent().remove();
            if ($('.InfoBasic').length == 0) $('#chkInfoB').parent().remove();
            if ($('.InfoMedium').length == 0) $('#chkInfoM').parent().remove();
            if ($('.InfoDetail').length == 0) $('#chkInfoD').parent().remove();
            if ($('.Debug').length + $('.LibDebug').length == 0) $('#chkDebug').parent().remove();
            if ($('.Stats').length == 0) $('#chkStats').parent().remove();
            if ($('.Verbose').length + $('.LibVerbose').length == 0) $('#chkVerbose').parent().remove();

            starts.goToEnd();

            $('.Start').each(function() { 
                var self = $(this); 
                var time = self.data('time');
                time = time.substr(0, time.length - 4);
                starts.data.push([time, self]); 
                $('#ulStarts').append($('<li onclick=\'starts.goTo(' + (starts.data.length - 1) + ')\'>' + time + '</li>'));
            });
            $('#ulStarts').append($('<li onclick=\'starts.goToEnd()\'>EOF</li>'));
            var lis = $('#ulStarts li');
            $('.displaywrap').scroll(function() { 
                var containerTop = -$('.display').position().top;
                lis.removeClass('liActive');
                for(var i = starts.data.length - 1; i >= 0; i--)
                {
                    var startTop = starts.data[i][1].position().top;
                    var top = (containerTop + startTop) - 100;
                    if (containerTop >= top) 
                    {
                        $(lis[i]).addClass('liActive');
                        return;
                    }
                }
            });
        });
        $('input[type=checkbox]').click(function() {
            if (this.id == 'chkError') $('.Error').toggle();
            if (this.id == 'chkWarning') $('.Warning').toggle();
            if (this.id == 'chkInfoB') $('.InfoBasic').toggle();
            if (this.id == 'chkInfoM') $('.InfoMedium').toggle();
            if (this.id == 'chkInfoD') $('.InfoDetail').toggle();
            if (this.id == 'chkDebug') { $('.Debug').toggle(); $('.LibDebug').toggle();}
            if (this.id == 'chkStats') $('.Stats').toggle();
            if (this.id == 'chkVerbose') { $('.Verbose').toggle(); $('.LibVerbose').toggle();}
            if (this.id == 'chkEmptyLines') $('.EmptyLine').toggle();
        });
    </script>
</div>
<div class='displaywrap'><div class='display'>
";
		string _preFormat = "<pre class='{0}'>{1}</pre>";
		string _preFormatWTime = "<pre class='{0}' data-time='{2}'>{1}</pre>";
		string _preFormatWType = "<pre class='{0}' data-type='{2}'>{1}</pre>";
		string _htmlEnd = "</pre></div></div></body></html>";
		#endregion

        #region Properties
        /// <summary>
        /// File name with path
        /// </summary>
        public string FileName { get; private set; }
        /// <summary>
        /// File creation date
        /// </summary>
        public DateTime FileDate { get; private set; }
        /// <summary>
        /// True if a new log file is created each day; otherwise, false
        /// </summary>
        public bool CreateByDay { get; private set; }
        /// <summary>
        /// True if a new log file is created when a maximum length is reached; otherwise, false.
        /// </summary>
        public bool UseMaxLength { get; private set; }
        /// <summary>
        /// Maximun length in bytes for a single file. Default value is 4Mb
        /// </summary>
        public long MaxLength { get; private set; } = 4194304L;
        #endregion

        #region .ctor
        /// <summary>
        /// Writes a simple log file
        /// </summary>
        /// <param name="fileName">File name with path</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
		public HtmlFileLogStorage(string fileName) :
            this(fileName, true)
        { }
        /// <summary>
        /// Writes a simple log file
        /// </summary>
        /// <param name="fileName">File name with path</param>
        /// <param name="createByDay">True if a new log file is created each day; otherwise, false</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
		public HtmlFileLogStorage(string fileName, bool createByDay) :
            this(fileName, createByDay, false)
        { }
        /// <summary>
        /// Writes a simple log file
        /// </summary>
        /// <param name="fileName">File name with path</param>
        /// <param name="createByDay">True if a new log file is created each day; otherwise, false</param>
        /// <param name="useMaxLength">True if a new log file is created when a maximum length is reached; otherwise, false.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
		public HtmlFileLogStorage(string fileName, bool createByDay, bool useMaxLength) :
            this(fileName, createByDay, useMaxLength, 4194304L)
        { }
        /// <summary>
        /// Writes a simple log file
        /// </summary>
        /// <param name="fileName">File name with path</param>
        /// <param name="createByDay">True if a new log file is created each day; otherwise, false</param>
        /// <param name="useMaxLength">True if a new log file is created when a maximum length is reached; otherwise, false.</param>
        /// <param name="maxLength">Maximun length in bytes for a single file. Default value is 4Mb</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
		public HtmlFileLogStorage(string fileName, bool createByDay, bool useMaxLength, long maxLength)
        {
            FileName = fileName;
            CreateByDay = createByDay;
            UseMaxLength = useMaxLength;
            MaxLength = maxLength;
            EnsureLogFile(fileName);

            Core.Status.Attach(collection =>
            {
                collection.Add(nameof(FileName), FileName);
                collection.Add(nameof(CreateByDay), CreateByDay);
                collection.Add(nameof(FileDate), FileDate);
                collection.Add(nameof(UseMaxLength), UseMaxLength);
                collection.Add(nameof(MaxLength), MaxLength);
                collection.Add("Current FileName", currentFileName);
            });
        }

        /// <summary>
        /// Destructor
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
		~HtmlFileLogStorage()
        {
            Core.Status.DeAttachObject(this);
            Dispose();
        }
        #endregion

        #region Private Methods
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void EnsureLogFile(string fileName)
        {
            bool dayHasChange = FileDate.Date != DateTime.Today;
            long fileLength = -1L;
            bool maxLengthReached = false;
            if (UseMaxLength)
            {
                fileLength = new FileInfo(currentFileName).Length;
                if (fileLength >= MaxLength)
                    maxLengthReached = true;
            }
            if (dayHasChange || maxLengthReached)
            {
                string oldFileName = string.Empty;
                if (dayHasChange && CreateByDay)
                {
                    currentFileName = Path.Combine(Path.GetDirectoryName(fileName), Path.GetFileNameWithoutExtension(fileName) + "_" + DateTime.Today.ToString("yyyy-MM-dd") + Path.GetExtension(fileName));
                    oldFileName = Path.Combine(Path.GetDirectoryName(fileName), Path.GetFileNameWithoutExtension(fileName) + "_" + FileDate.Date.ToString("yyyy-MM-dd") + Path.GetExtension(fileName));
                    numbersOfFiles = 0;
                }
                else
                {
                    currentFileName = fileName;
                    oldFileName = fileName;
                }
                if (maxLengthReached)
                {
                    oldFileName = currentFileName;
                    numbersOfFiles++;
                    currentFileName = Path.Combine(Path.GetDirectoryName(currentFileName), Path.GetFileNameWithoutExtension(currentFileName) + "_" + FileDate.Date.ToString("yyyy-MM-dd") + "." + numbersOfFiles + Path.GetExtension(currentFileName));
                }

                #region Remove previous
                try
                {
					sWriter?.WriteLine(_htmlEnd);
                    sWriter?.Dispose();
                    sWriter = null;
                }
                catch
                {
                    // ignored
                }
                if (logStreams.TryRemove(oldFileName, out var oldWriter))
                {
                    try
                    {
                        oldWriter?.Dispose();
                        oldWriter = null;
                    }
                    catch
                    {
                        // ignored
                    }
                }
                #endregion

                #region Load Writer
                sWriter = logStreams.GetOrAdd(currentFileName, fname =>
                {
                    var folder = Path.GetDirectoryName(fname);
                    if (!string.IsNullOrWhiteSpace(folder) && !Directory.Exists(folder))
                        Directory.CreateDirectory(folder);
					var alreadyExist = File.Exists(fname);
                    var sw = new StreamWriter(new FileStream(fname, FileMode.Append, FileAccess.Write, FileShare.ReadWrite))
                    {
                        AutoFlush = true
                    };
					if (!alreadyExist)
						sw.WriteLine(_html);
                    return sw;
                });
                if (File.Exists(currentFileName))
                    FileDate = File.GetCreationTime(currentFileName);
                #endregion
            }
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static string GetExceptionDescription(SerializableException itemEx)
        {
            var desc = string.Format("\tType: {0}\r\n\tMessage: {1}\r\n\tStack: {2}\r\n", itemEx.ExceptionType, System.Security.SecurityElement.Escape(itemEx.Message).Replace("\r", "\\r").Replace("\n", "\\n"), itemEx.StackTrace);
            if (itemEx.InnerException != null)
                desc += GetExceptionDescription(itemEx.InnerException);
            return desc;
        }
        #endregion

        /// <summary>
        /// Writes a log item to the storage
        /// </summary>
        /// <param name="item">Log Item</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Write(ILogItem item)
        {
            EnsureLogFile(FileName);
            if (sWriter != null)
            {
                var time = item.Timestamp.GetTimeSpanFormat();
                var format = _preFormat;
                if (firstWrite)
                {
                    lock (sWriter)
                    {
                        sWriter.WriteLine(_preFormat, "EmptyLine", "<br/>");
						sWriter.WriteLine(_preFormat, "EmptyLine", "<br/>");
						sWriter.WriteLine(_preFormat, "EmptyLine", "<br/>");
						sWriter.WriteLine(_preFormat, "EmptyLine", "<br/>");
						sWriter.WriteLine(_preFormat, "EmptyLine", "<br/>");
						sWriter.WriteLine(_preFormatWTime, "Start", "&#8615; START &#8615;", time);
                        sWriter.Flush();
                    }
                    firstWrite = false;
                }
                var sbuilder = new StringBuilder(128);
                sbuilder.Append(time);
                sbuilder.AppendFormat(" ({0:000}) ", item.ThreadId);
                sbuilder.AppendFormat("{0, 10}: ", item.Level);

                if (!string.IsNullOrEmpty(item.GroupName))
                    sbuilder.Append(item.GroupName + " ");

                if (item.LineNumber > 0)
                    sbuilder.AppendFormat("&lt;{0};{1:000}&gt; ", string.IsNullOrEmpty(item.TypeName) ? string.Empty : item.TypeName, item.LineNumber);
                else if (!string.IsNullOrEmpty(item.TypeName))
                {
                    sbuilder.Append("&lt;" + item.TypeName + "&gt; ");
                    format = _preFormatWType;
                }

                if (!string.IsNullOrEmpty(item.Code))
                    sbuilder.Append("[" + item.Code + "] ");
                
                sbuilder.Append(System.Security.SecurityElement.Escape(item.Message));

                if (item.Exception != null)
                {
                    sbuilder.Append("\r\nExceptions:\r\n");
                    sbuilder.Append(GetExceptionDescription(item.Exception));
                }
                string line = sbuilder.ToString();

                lock (sWriter)
                {
                    sWriter.WriteLine(format, item.Level, line, item.TypeName);
                    sWriter.Flush();
                }
            }
        }
        /// <summary>
        /// Writes a log item empty line
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteEmptyLine()
        {
            lock (sWriter)
            {
				sWriter.WriteLine(_preFormat, "EmptyLine", "<br/>");
                sWriter.Flush();
            }
        }

        /// <summary>
        /// Dispose the current object resources
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Dispose()
        {
            try
            {
				sWriter?.WriteLine(_preFormat, "End", "&#8613; END &#8613;");
                sWriter?.Flush();
                sWriter?.Dispose();
                sWriter = null;
            }
            catch
            {
                // ignored
            }
            if (!logStreams.TryRemove(currentFileName, out var oldWriter)) return;
            try
            {
                oldWriter?.Dispose();
                oldWriter = null;
            }
            catch
            {
                // ignored
            }
        }
    }
}
