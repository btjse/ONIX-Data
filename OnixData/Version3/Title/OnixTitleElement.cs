﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnixData.Version3.Title
{
    /// <remarks/>
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class OnixTitleElement
    {
        #region CONSTANTS

        public const int CONST_TITLE_TYPE_PRODUCT = 1;
        public const int CONST_TITLE_TYPE_COLLECTION = 2;
        public const int CONST_TITLE_TYPE_SUB_COLL = 3;
        public const int CONST_TITLE_TYPE_SUB_ITEM = 4;

        #endregion

        public OnixTitleElement()
        {
            TitleElementLevel = -1;
            TitleText = TitlePrefix = TitleWithoutPrefix = "";
        }

        private int titleElementLevelField;
        private string titleTextField;
        private string titlePrefixField;
        private string titleWithoutPrefixField;

        #region Reference Tags

        public string Title
        {
            get
            {
                string sTitle = "";

                if (!String.IsNullOrEmpty(TitleText))
                    sTitle = TitleText;
                else if (!String.IsNullOrEmpty(TitleWithoutPrefix))
                    sTitle = TitlePrefix + " " + TitleWithoutPrefix;

                return sTitle;
            }
        }

        /// <remarks/>
        public int TitleElementLevel
        {
            get
            {
                return this.titleElementLevelField;
            }
            set
            {
                this.titleElementLevelField = value;
            }
        }

        /// <remarks/>
        public string TitleText
        {
            get
            {
                return this.titleTextField;
            }
            set
            {
                this.titleTextField = value;
            }
        }

        /// <remarks/>
        public string TitlePrefix
        {
            get
            {
                return this.titlePrefixField;
            }
            set
            {
                this.titlePrefixField = value;
            }
        }

        /// <remarks/>
        public string TitleWithoutPrefix
        {
            get
            {
                return this.titleWithoutPrefixField;
            }
            set
            {
                this.titleWithoutPrefixField = value;
            }
        }

        #endregion

        #region Short Tags

        /// <remarks/>
        public int x409
        {
            get
            {
                return this.titleElementLevelField;
            }
            set
            {
                this.titleElementLevelField = value;
            }
        }

        /// <remarks/>
        public string b203
        {
            get
            {
                return this.titleTextField;
            }
            set
            {
                this.titleTextField = value;
            }
        }

        /// <remarks/>
        public string b030
        {
            get
            {
                return this.titlePrefixField;
            }
            set
            {
                this.titlePrefixField = value;
            }
        }

        /// <remarks/>
        public string b031
        {
            get
            {
                return this.titleWithoutPrefixField;
            }
            set
            {
                this.titleWithoutPrefixField = value;
            }
        }

        #endregion
    }
}
