// Copyright (c) All contributors. All rights reserved. Licensed under the MIT license.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Xml.Linq;

namespace Arc.Text
{
    public class C4
    {
        private object cs; // critical section
        private Utf16Hashtable<string> currentCultureTable; // Current culture data (name, string).
        private Utf16Hashtable<string> defaultCultureTable; // Default culture data (name, string).
        private Utf16Hashtable<Utf16Hashtable<string>> cultureTable; // Culture and data (culture, Utf16Hashtable<string>).
        private string? currentCulture; // Current culture

        public C4()
        {
            this.cs = new object(); // critical section

            var table = new Utf16Hashtable<string>();
            this.currentCultureTable = table;
            this.defaultCultureTable = table;
            this.cultureTable = new Utf16Hashtable<Utf16Hashtable<string>>();
        }

        public static C4 Instance { get; } = new C4();

        public string ErrorText => "C4 error"; // Error message.

        public int MaxIdLength => 256; // The maximum length of a identifier.

        public int MaxTextLength => 16 * 1024; // The maximum length of a text.

        public uint MaxSize => 4 * 1024 * 1024; // Max data size, 4MB

        public CultureInfo? CurrentCulture { get; private set; }

        /// <summary>
        /// Get a string that matches the identifier.
        /// </summary>
        /// <param name="identifier">The identifier.</param>
        /// <returns>Returns a string. If no string is found, the return value is the identifier.</returns>
        public string this[string? identifier]
        {
            get
            {
                if (identifier == null)
                {
                    return this.ErrorText;
                }

                string? result;
                if (this.currentCultureTable.TryGetValue(identifier, out result))
                {
                    return result;
                }

                if (this.defaultCultureTable.TryGetValue(identifier, out result))
                {
                    return result;
                }

                return identifier;
            }
        }

        /// <summary>
        /// Get a string that matches the identifier.
        /// </summary>
        /// <param name="identifier">The identifier.</param>
        /// <returns>Returns a string. If no string is found, the return value is null.</returns>
        public string? Get(string? identifier)
        {
            if (identifier == null)
            {
                return this.ErrorText;
            }

            string? result;
            if (this.currentCultureTable.TryGetValue(identifier, out result))
            {
                return result;
            }

            if (this.defaultCultureTable.TryGetValue(identifier, out result))
            {
                return result;
            }

            return null;
        }

        public void Load(string culture, string filename, bool clearFlag = false)
        { // clearFlag 1:clear data
            // Exception: ArgumentException, FileNotFoundException, OverflowException
            // Exception: InvalidDataException
            if (filename == null)
            {
                throw new ArgumentException();
            }

            try
            {
                using (var fs = File.OpenRead(filename))
                {
                    lock (this.cs)
                    {
                        this.Load(culture, fs, clearFlag);
                    }
                }
            }
            catch (FileNotFoundException e)
            {
                throw e;
            }
        }

        public void LoadStream(string culture, Stream stream, bool clearFlag = false)
        { // flag 1:clear data
            // Exception: ArgumentException, FileNotFoundException, OverflowException
            // Exception: InvalidDataException
            if (stream == null)
            {
                throw new ArgumentException();
            }

            lock (this.cs)
            {
                this.Load(culture, stream, clearFlag);
            }
        }

#if !NETFX_CORE
        /// <summary>
        /// Load from assembly.
        /// </summary>
        /// <param name="culture">The target culture.</param>
        /// <param name="assemblyname">The assembly name.</param>
        /// <param name="clearFlag">Initialize string data and load.</param>
        public void LoadAssembly(string culture, string assemblyname, bool clearFlag = false)
        {
            var asm = System.Reflection.Assembly.GetExecutingAssembly();
            using (var stream = asm.GetManifestResourceStream(asm.GetName().Name + "." + assemblyname))
            {
                if (stream == null)
                {
                    throw new FileNotFoundException();
                }

                lock (this.cs)
                {
                    this.Load(culture, stream, clearFlag);
                }
            }
        }
#endif

        /// <summary>
        /// Set the default culture.
        /// </summary>
        /// <param name="defaultCulture">A string of the default culture.</param>
        public void SetDefaultCulture(string defaultCulture)
        {
            lock (this.cs)
            {
                this.defaultCultureTable = this.cultureTable[defaultCulture];
                if (this.currentCultureTable == null)
                {
                    this.currentCultureTable = this.defaultCultureTable;
                }
            }
        }

        public void SetCulture(string culture)
        { // set current culture. cultureが見つからない場合は、KeyNotFoundException
            string ci;
            switch (culture)
            {
                case "ja":
                    ci = "ja-JP";
                    break;
                case "en":
                    ci = "en-US";
                    break;
                default:
                    ci = culture;
                    break;
            }

            this.CurrentCulture = new CultureInfo(ci);

            lock (this.cs)
            {
                try
                {
                    this.currentCultureTable = this.cultureTable[culture];
                    this.currentCulture = culture;
                }
                catch (KeyNotFoundException e)
                {
                    throw e;
                }
            }
        }

        /// <summary>
        /// Get current culture.
        /// </summary>
        /// <param name="culture">culture</param>
        public void GetCulture(out string? culture)
        {
            lock (this.cs)
            {
                culture = this.currentCulture;
            }
        }

        public void SetErrorText(string text)
        { // set error text.  データが見つからないときのメッセージを設定する。
            if (text == null)
            {
                throw new ArgumentException();
            }

            lock (this.cs)
            {
                this.ErrorText = text;
            }
        }

        private void Load(string culture, Stream stream, bool clearFlag)
        { // load xml
            if (stream.Length > this.MaxSize)
            {
                throw new OverflowException();
            }

            if (culture.Length > this.MaxIdLength)
            {
                throw new OverflowException();
            }

            Dictionary<string, string>? data;
            this.cultureTable.TryGetValue(culture, out data); // get culture data
            if (data == null)
            {
                data = new Dictionary<string, string>();
                this.cultureTable[culture] = data;
            }
            else
            {
                if (clearFlag)
                { // clear
                    data.Clear();
                }
            }

            // var doc = new XmlDocument();
            try
            {
                // doc.Load(stream);
                var root = XElement.Load(stream);
                foreach (var x in root.Elements("string"))
                {
                    this.LoadString2(data, x);
                }
            }
            catch
            {
                throw new InvalidDataException();
            }

            return;
        }

        private void LoadString2(Dictionary<string, string> data, XElement element)
        {
            // get text
            /*foreach(var node in element.Nodes())
            {
                if ((node.NodeType == XmlNodeType.Text) || (node.NodeType == XmlNodeType.CDATA))
                {
                    text = node.Value;
                    goto _load_string_get;
                }
            }*/
            var text = element.Value;

            // get name
            var name = element.Attribute("name")?.Value;
            if (name == null)
            {// no name
                return;
            }

            // check text length
            if (text.Length > this.MaxTextLength)
            {
                return;
            }

            // set
            data[name] = text;
        }
    }
}
