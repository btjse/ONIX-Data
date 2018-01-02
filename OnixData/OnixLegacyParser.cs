﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Serialization;
using System.Xml.Schema;

using OnixData.Extensions;
using OnixData.Legacy;

namespace OnixData
{
    /// <summary>
    /// 
    /// This class serves as a way to parse files of the ONIX 2.1 standard (and earlier).
    /// You can use this class to either:
    /// 
    /// a.) Deserialize and load the entire file into memory
    /// b.) Enumerate through the file record by record, loading each into memory one at a time
    ///     
    /// </summary>
    public class OnixLegacyParser : IDisposable, IEnumerable
    {
        #region CONSTANTS

        private const int    CONST_MSG_REFERENCE_LENGTH = 500;
        private const int    CONST_BLOCK_COUNT_SIZE     = 50000000;

        private const string CONST_ONIX_MESSAGE_REFERENCE_TAG = "ONIXMessage";
        private const string CONST_ONIX_MESSAGE_SHORT_TAG     = "ONIXmessage";

        private const string CONST_ONIX_HEADER_REFERENCE_TAG = "Header";
        private const string CONST_ONIX_HEADER_SHORT_TAG     = "header";

        #endregion

        private bool              DebugFlag         = true;
        private bool              ParserRefVerFlag  = false;
        private bool              ParserRVWFlag     = false;
        private bool              PerformValidFlag  = false;
        private StringBuilder     ParserFileContent = null;
        private FileInfo          ParserFileInfo    = null;
        private XmlReader         LegacyOnixReader  = null;
        private OnixLegacyMessage LegacyOnixMessage = null;

        public bool PerformValidation
        {
            get { return this.PerformValidFlag; }
        }

        public bool ReferenceVersion
        {
            get { return this.ParserRefVerFlag; }
        }

        public OnixLegacyParser(FileInfo LegacyOnixFilepath, 
                                    bool ExecuteValidation,
                                    bool PreprocessOnixFile = true, 
                                    bool LoadEntireFileIntoMemory = false)
        {
            if (!File.Exists(LegacyOnixFilepath.FullName))
                throw new Exception("ERROR!  File(" + LegacyOnixFilepath + ") does not exist.");

            this.ParserFileInfo   = LegacyOnixFilepath;
            this.ParserRVWFlag    = true;
            this.PerformValidFlag = ExecuteValidation;

            if (PreprocessOnixFile)
                LegacyOnixFilepath.ReplaceIsoLatinEncodings(true);

            this.LegacyOnixReader = CreateXmlReader(this.ParserFileInfo, this.ParserRVWFlag, this.PerformValidFlag);

            bool   ReferenceVersion = DetectVersionReference(LegacyOnixFilepath);
            string sOnixMsgTag      = ReferenceVersion ? CONST_ONIX_MESSAGE_REFERENCE_TAG : CONST_ONIX_MESSAGE_SHORT_TAG;

            this.ParserRefVerFlag = ReferenceVersion;

            if (LoadEntireFileIntoMemory)
            {
                this.LegacyOnixMessage = 
                    CreateOnixXmlMsgSerializer(sOnixMsgTag).Deserialize(this.LegacyOnixReader) as OnixLegacyMessage;
            }
            else
                this.LegacyOnixMessage = null;
        }

        public OnixLegacyParser(bool ExecuteValidation,
                            FileInfo LegacyOnixFilepath,
                                bool ReferenceVersion,
                                bool PreprocessOnixFile = true,
                                bool LoadEntireFileIntoMemory = false)
        {
            string sOnixMsgTag = ReferenceVersion ? CONST_ONIX_MESSAGE_REFERENCE_TAG : CONST_ONIX_MESSAGE_SHORT_TAG;

            if (!File.Exists(LegacyOnixFilepath.FullName))
                throw new Exception("ERROR!  File(" + LegacyOnixFilepath + ") does not exist.");

            this.ParserRefVerFlag = ReferenceVersion;
            this.ParserFileInfo   = LegacyOnixFilepath;
            this.ParserRVWFlag    = true;
            this.PerformValidFlag = ExecuteValidation;

            if (PreprocessOnixFile)
                LegacyOnixFilepath.ReplaceIsoLatinEncodings(true);

            this.LegacyOnixReader = CreateXmlReader(this.ParserFileInfo, this.ParserRVWFlag, this.PerformValidFlag);

            if (LoadEntireFileIntoMemory)
            {
                this.LegacyOnixMessage =
                    CreateOnixXmlMsgSerializer(sOnixMsgTag).Deserialize(this.LegacyOnixReader) as OnixLegacyMessage;
            }
            else
                this.LegacyOnixMessage = null;
        }

        public OnixLegacyParser(string   LegacyOnixContent, 
                                    bool ExecuteValidation,
                                    bool PreprocessOnixFile = true, 
                                    bool LoadEntireFileIntoMemory = false)
        {
            if (String.IsNullOrEmpty(LegacyOnixContent))
                throw new Exception("ERROR!  ONIX content provided is empty.");

            this.ParserFileContent = new StringBuilder(LegacyOnixContent);
            this.ParserRVWFlag     = true;
            this.PerformValidFlag  = ExecuteValidation;

            if (PreprocessOnixFile)
                ParserFileContent.ReplaceIsoLatinEncodings();

            this.LegacyOnixReader = CreateXmlReader(this.ParserFileContent, this.ParserRVWFlag, this.PerformValidFlag);

            bool   ReferenceVersion = DetectVersionReference(ParserFileContent);
            string sOnixMsgTag      = ReferenceVersion ? CONST_ONIX_MESSAGE_REFERENCE_TAG : CONST_ONIX_MESSAGE_SHORT_TAG;

            this.ParserRefVerFlag = ReferenceVersion;

            if (LoadEntireFileIntoMemory)
            {
                this.LegacyOnixMessage =
                    CreateOnixXmlMsgSerializer(sOnixMsgTag).Deserialize(this.LegacyOnixReader) as OnixLegacyMessage;
            }
            else
                this.LegacyOnixMessage = null;
        }

        public OnixLegacyParser(bool ExecuteValidation,
                              string LegacyOnixContent,
                                bool ReferenceVersion,
                                bool PreprocessOnixFile = true,
                                bool LoadEntireFileIntoMemory = false)
        {
            string sOnixMsgTag = ReferenceVersion ? CONST_ONIX_MESSAGE_REFERENCE_TAG : CONST_ONIX_MESSAGE_SHORT_TAG;

            if (String.IsNullOrEmpty(LegacyOnixContent))
                throw new Exception("ERROR!  ONIX content provided is empty.");

            this.ParserRefVerFlag  = ReferenceVersion;
            this.ParserFileContent = new StringBuilder(LegacyOnixContent);
            this.ParserRVWFlag     = true;
            this.PerformValidFlag  = ExecuteValidation;

            if (PreprocessOnixFile)
                ParserFileContent.ReplaceIsoLatinEncodings();

            this.LegacyOnixReader = CreateXmlReader(this.ParserFileContent, this.ParserRVWFlag, this.PerformValidFlag);

            if (LoadEntireFileIntoMemory)
            {
                this.LegacyOnixMessage =
                    CreateOnixXmlMsgSerializer(sOnixMsgTag).Deserialize(this.LegacyOnixReader) as OnixLegacyMessage;
            }
            else
                this.LegacyOnixMessage = null;
        }

        /// <summary>
        /// 
        /// This method will prepare the XmlReader that we will use to read the ONIX XML file.
        /// 
        /// <param name="CurrOnixFilepath">The path to the ONIX file</param>
        /// <param name="ReportValidationWarnings">The indicator for whether we should report validation warnings to the caller</param>
        /// <param name="ExecutionValidation">The indicator for whether or not the ONIX file should be validated</param>
        /// <returns>The XmlReader that will be used to read the ONIX file</returns>
        /// </summary>
        static public XmlReader CreateXmlReader(FileInfo LegacyOnixFilepath, bool ReportValidationWarnings, bool ExecutionValidation)
        {
            bool          bUseXSD       = true;
            bool          bUseDTD       = false;
            StringBuilder InvalidErrMsg = new StringBuilder();
            XmlReader     OnixXmlReader = null;

            XmlReaderSettings settings = new XmlReaderSettings();

            settings.ValidationType = ValidationType.None;
            settings.DtdProcessing  = DtdProcessing.Ignore;

            if (ExecutionValidation)
            {
                settings.ConformanceLevel = ConformanceLevel.Document;

                if (bUseXSD)
                {
                    /*
                     * NOTE: XSD Validation does not appear to be working correctly yet
                     * 
                    XmlSchemaSet schemas = new XmlSchemaSet();

                    string XsdFilepath = "";

                    if (ParserRefVerFlag)
                        XsdFilepath = @"C:\ONIX_XSD\2.1\ONIX_BookProduct_Release2.1_reference.xsd";
                    else
                        XsdFilepath = @"C:\ONIX_XSD\2.1\ONIX_BookProduct_Release2.1_short.xsd";

                    schemas.Add(null, XmlReader.Create(new StringReader(File.ReadAllText(XsdFilepath))));

                    settings.Schemas        = schemas;
                    settings.ValidationType = ValidationType.Schema;                

                    if (ReportValidationWarnings)
                        settings.ValidationFlags |= XmlSchemaValidationFlags.ReportValidationWarnings;
                    else
                        settings.ValidationFlags &= ~(XmlSchemaValidationFlags.ReportValidationWarnings);

                    // settings.ValidationFlags |= XmlSchemaValidationFlags.ProcessInlineSchema;
                    */
                }

                /*
                 * NOTE: DTD Validation does not appear that it will ever work correctly on the .NET platform
                 * 
                if (bUseDTD)
                {
                    settings.DtdProcessing  = DtdProcessing.Parse;
                    settings.ValidationType = ValidationType.DTD;

                    settings.ValidationFlags |= XmlSchemaValidationFlags.AllowXmlAttributes;

                    System.Xml.XmlResolver newXmlResolver = new System.Xml.XmlUrlResolver();
                    newXmlResolver.ResolveUri(new Uri("C:\\ONIX_DTD\\2.1\\short"), "onix-international.dtd");
                    settings.XmlResolver = newXmlResolver;

                    settings.ValidationEventHandler += new ValidationEventHandler(delegate(object sender, ValidationEventArgs args)
                    {
                        // InvalidErrMsg.AppendLine(args.Message);
                        throw new Exception(args.Message);
                    });
                }
                */
            }

            OnixXmlReader = XmlReader.Create(LegacyOnixFilepath.FullName, settings);

            return OnixXmlReader;
        }

        static public XmlReader CreateXmlReader(StringBuilder LegacyOnixContent, bool ReportValidationWarnings, bool ExecutionValidation)
        {
            bool bUseXSD = true;

            StringBuilder InvalidErrMsg = new StringBuilder();
            XmlReader     OnixXmlReader = null;

            XmlReaderSettings settings = new XmlReaderSettings();

            settings.ValidationType = ValidationType.None;
            settings.DtdProcessing  = DtdProcessing.Ignore;

            if (ExecutionValidation)
            {
                settings.ConformanceLevel = ConformanceLevel.Document;

                if (bUseXSD)
                { /* NOTE: XSD Validation does not appear to be working correctly yet */ }

                /* NOTE: DTD Validation does not appear that it will ever work correctly on the .NET platform */
            }

            OnixXmlReader = XmlReader.Create(new StringReader(LegacyOnixContent.ToString()), settings);

            return OnixXmlReader;
        }

        static public XmlSerializer CreateOnixXmlMsgSerializer(string psOnixMsgTag)
        {
            var OnixXmlSerializer = new XmlSerializer(typeof(OnixLegacyMessage), new XmlRootAttribute(psOnixMsgTag));

            return OnixXmlSerializer;
        }

        /// <summary>
        /// 
        /// This property will return the header of an ONIX file.  If the file has already been loaded 
        /// into memory, it will extract the header from the internal member reader.  If not, it will 
        /// open the file temporarily and extract it from there.
        /// 
        /// <returns>The header of the ONIX file</returns>
        /// </summary>
        public OnixLegacyHeader MessageHeader
        {
            get
            {
                string sOnixHdrTag = 
                    this.ParserRefVerFlag ? CONST_ONIX_HEADER_REFERENCE_TAG : CONST_ONIX_HEADER_SHORT_TAG;

                OnixLegacyHeader LegacyHeader = new OnixLegacyHeader();

                if (this.LegacyOnixMessage != null)
                    LegacyHeader = this.LegacyOnixMessage.Header;
                else if (this.LegacyOnixReader != null)
                {
                    XmlDocument XMLDoc = new XmlDocument();
                    XMLDoc.Load(this.LegacyOnixReader);

                    XmlNodeList HeaderList = XMLDoc.GetElementsByTagName(sOnixHdrTag);

                    if ((HeaderList != null) && (HeaderList.Count > 0))
                    {
                        XmlNode HeaderNode  = HeaderList.Item(0);
                        string  sHeaderBody = HeaderNode.OuterXml;

                        LegacyHeader = 
                            new XmlSerializer(typeof(OnixLegacyHeader), new XmlRootAttribute(sOnixHdrTag))
                            .Deserialize(new StringReader(sHeaderBody)) as OnixLegacyHeader;
                    }
                }

                return LegacyHeader;
            }
        }

        public OnixLegacyMessage Message
        {
            get
            {
                return LegacyOnixMessage;
            }
        }

        public void Dispose()
        {
            if (this.LegacyOnixReader != null)
            {
                this.LegacyOnixReader.Close();
                this.LegacyOnixReader = null;
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return (IEnumerator) GetEnumerator();
        }

        public OnixLegacyEnumerator GetEnumerator()
        {
            if (this.ParserFileInfo != null)
                return new OnixLegacyEnumerator(this, this.ParserFileInfo);
            else if (this.ParserFileContent != null)
                return new OnixLegacyEnumerator(this, this.ParserFileContent);
            else
                return null;
        }

        /// <summary>
        /// 
        /// This method was intended to provide the functionality of validating a legacy ONIX file against its 
        /// respective DTD/XSD.  Unfortunately, though, the .NET platform does not seem to be compatible with 
        /// parsing and understanding a complex schema as specified in the ONIX standard.  So, it seems that this
        /// method will never come into fruition.
        /// 
        /// <returns>The Boolean that indicates whether or not the ONIX file is valid</returns>
        /// </summary>
        public bool ValidateFile()
        {
            bool ValidOnixFile = true;

            /*
             * NOTE: XSD Validation is proving to be consistently problematic
             * 
            try
            {
                XmlReaderSettings TempReaderSettings = new XmlReaderSettings() { DtdProcessing = DtdProcessing.Ignore };

                using (XmlReader TempXmlReader = XmlReader.Create(ParserFileInfo.FullName, TempReaderSettings))
                {
                    XDocument OnixDoc = XDocument.Load(TempXmlReader);

                    XmlSchemaSet schemas = new XmlSchemaSet();

                    string XsdFilepath = "";

                    if (ParserRefVerFlag)
                        XsdFilepath = @"C:\ONIX_XSD\2.1\ONIX_BookProduct_Release2.1_reference.xsd";
                    else
                        XsdFilepath = @"C:\ONIX_XSD\2.1\ONIX_BookProduct_Release2.1_short.xsd";

                    schemas.Add(null, XmlReader.Create(new StringReader(File.ReadAllText(XsdFilepath))));

                    schemas.Compile();

                    OnixDoc.Validate(schemas, (o, e) =>
                    {
                        Console.WriteLine("{0}", e.Message);
                        ValidOnixFile = false;
                    });
                }
            }
            catch (Exception ex)
            {
                ValidOnixFile = false;
            }
            */

            return ValidOnixFile;
        }

        #region Support Methods

        /// <summary>
        /// 
        /// This method will help determine whether the XML structure of an ONIX file belongs to the 'Reference' type 
        /// of ONIX (i.e., verbose tags) or the 'Short' type of ONIX (i.e., alphanumeric tags).
        /// 
        /// <param name="LegacyOnixFilepath">The path to the ONIX file</param>
        /// <returns>The Boolean that indicates whether or not the ONIX file belongs to the 'Reference' type</returns>
        /// </summary>
        public bool DetectVersionReference(FileInfo LegacyOnixFileInfo)
        {
            bool bReferenceVersion = true;

            if (LegacyOnixFileInfo.Length < CONST_MSG_REFERENCE_LENGTH)
                throw new Exception("ERROR!  ONIX File is smaller than expected!");

            byte[] buffer = new byte[CONST_MSG_REFERENCE_LENGTH];
            using (FileStream fs = new FileStream(LegacyOnixFileInfo.FullName, FileMode.Open, FileAccess.Read))
            {
                fs.Read(buffer, 0, buffer.Length);
                fs.Close();
            }

            string sRefMsgTag = "<" + CONST_ONIX_MESSAGE_REFERENCE_TAG;
            string sFileHead  = Encoding.Default.GetString(buffer);
            if (sFileHead.Contains(sRefMsgTag))
                bReferenceVersion = true;
            else
                bReferenceVersion = false;

            return bReferenceVersion;
        }

        public bool DetectVersionReference(StringBuilder LegacyOnixContent)
        {
            bool bReferenceVersion = true;

            if (LegacyOnixContent.Length < CONST_MSG_REFERENCE_LENGTH)
                throw new Exception("ERROR!  ONIX File is smaller than expected!");

            string sRefMsgTag = "<" + CONST_ONIX_MESSAGE_REFERENCE_TAG;
            string sFileHead  = LegacyOnixContent.ToString().Substring(0, CONST_MSG_REFERENCE_LENGTH);
            if (sFileHead.Contains(sRefMsgTag))
                bReferenceVersion = true;
            else
                bReferenceVersion = false;

            return bReferenceVersion;
        }

        #endregion
    }

    /// <summary>
    ///
    /// This class can be useful in the case that one wants to iterate through an ONIX file, even if it has a bad record due to:
    /// 
    /// a.) incorrect XML syntax
    /// b.) invalid text within a XML document (like certain hexadecimal Unicode)
    /// d.) improper tag placement
    /// d.) invalid data types
    /// 
    /// In that way, the user of the class can investigate each record on a case-by-case basis, and the file can be processed
    /// without a sole record preventing the rest of the file from being handled.
    /// 
    /// NOTE: It is still recommended that the files be validated through an alternate process before using this class.
    /// 
    /// </summary> 
    public class OnixLegacyEnumerator : IDisposable, IEnumerator
    {
        private OnixLegacyParser OnixParser = null;
        private XmlReader        OnixReader = null;
        
        private int                CurrentIndex      = -1;
        private string             ProductXmlTag     = null;
        private List<string>       OtherTextList     = new List<string>();
        private XmlDocument        OnixDoc           = null;
        private XmlNodeList        ProductList       = null;
        private OnixLegacyProduct  CurrentRecord     = null;
        private XmlSerializer      ProductSerializer = null;

        public OnixLegacyEnumerator(OnixLegacyParser ProvidedParser, FileInfo LegacyOnixFilepath) 
        {
            this.ProductXmlTag = ProvidedParser.ReferenceVersion ? "Product" : "product";

            this.OnixParser = ProvidedParser;
            this.OnixReader = OnixLegacyParser.CreateXmlReader(LegacyOnixFilepath, false, ProvidedParser.PerformValidation);

            ProductSerializer = new XmlSerializer(typeof(OnixLegacyProduct), new XmlRootAttribute(this.ProductXmlTag));

            ProductSerializer.UnknownElement += (s, e) =>
            {
                if (((e.Element.LocalName == "Text") || (e.Element.LocalName == "d104")) && 
                    e.ObjectBeingDeserialized is OnixLegacyOtherText)
                {
                    OtherTextList.Add(e.Element.InnerText);
                }
            };
        }

        public OnixLegacyEnumerator(OnixLegacyParser ProvidedParser, StringBuilder LegacyOnixContent)
        {
            this.ProductXmlTag = ProvidedParser.ReferenceVersion ? "Product" : "product";

            this.OnixParser = ProvidedParser;
            this.OnixReader = OnixLegacyParser.CreateXmlReader(LegacyOnixContent, false, ProvidedParser.PerformValidation);

            ProductSerializer = new XmlSerializer(typeof(OnixLegacyProduct), new XmlRootAttribute(this.ProductXmlTag));
        }

        public void Dispose()
        {
            if (this.OnixReader != null)
            {
                this.OnixReader.Close();
                this.OnixReader = null;
            }
        }

        public bool MoveNext()
        {
            bool bResult = true;

            if (OnixDoc == null)
            {
                this.OnixDoc = new XmlDocument();
                this.OnixDoc.Load(this.OnixReader);

                this.ProductList = this.OnixDoc.GetElementsByTagName(this.ProductXmlTag);
            }

            if (++CurrentIndex < this.ProductList.Count)
            {
                string sInputXml = this.ProductList[CurrentIndex].OuterXml;

                try
                {
                    CurrentRecord =
                        this.ProductSerializer.Deserialize(new StringReader(sInputXml)) as OnixLegacyProduct;

                    if ((CurrentRecord != null) && 
                        (CurrentRecord.OnixOtherTextList != null) && 
                        (CurrentRecord.OnixOtherTextList.Length > 0))
                    {
                        int nOTCount = CurrentRecord.OnixOtherTextList.Length;

                        for (int nIdx = 0; nIdx < OtherTextList.Count; ++nIdx)
                        {
                            if (nIdx < nOTCount)
                            {
                                OnixLegacyOtherText TempText = CurrentRecord.OnixOtherTextList[nIdx];
                                TempText.Text = OtherTextList[nIdx];
                            }
                        }
                    }

                    OtherTextList.Clear();
                }
                catch (Exception ex)
                {
                    CurrentRecord = new OnixLegacyProduct();

                    CurrentRecord.SetParsingError(ex);
                    CurrentRecord.SetInputXml(sInputXml);
                }
            }
            else
                bResult = false;

            return bResult;
        }

        public void Reset()
        {
            return;
        }

        public object Current
        {
            get
            {
                return CurrentRecord;
            }
        }
    }
}
