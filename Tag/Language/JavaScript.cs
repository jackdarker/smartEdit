﻿using System.Collections.Generic;
using System;
using System.Windows.Forms;

namespace NppProject.Tag
{
    public class JavaScriptTag : ITag
    {
        public JavaScriptTag(string srcFile, int lineNo, string tagName, TagType tagType, string belongTo)
            : base(srcFile, lineNo, tagName, tagType, AccessType.Other, belongTo, "")
        { }

        public override Language Lang { get { return Language.JavaScript; } }

        public override bool BindToTreeNode(TreeNode node)
        {
            node.Text = TagName;
            node.ToolTipText = SourceFile;
            switch (TagType)
            {
                case TagType.JavaScript_Class:
                    node.ImageIndex = node.SelectedImageIndex = Resource.ClassViewIcon_JavaScript_Class;
                    break;
                case TagType.JavaScript_Function:
                    node.ImageIndex = node.SelectedImageIndex = Resource.ClassViewIcon_JavaScript_Function;
                    break;
                case TagType.JavaScript_GlobalVar:
                    node.ImageIndex = node.SelectedImageIndex = Resource.ClassViewIcon_JavaScript_GlobalVar;
                    break;
                case TagType.JavaScript_Method:
                    node.ImageIndex = node.SelectedImageIndex = Resource.ClassViewIcon_JavaScript_Method;
                    break;
                case TagType.JavaScript_Property:
                    node.ImageIndex = node.SelectedImageIndex = Resource.ClassViewIcon_JavaScript_Property;
                    break;
            }
            return true;
        }
    }

    public class JavaScriptTagParser : TagParser
    {
        public override Language Lang { get { return Language.JavaScript; } }

        string _args = "--excmd=number --fields=aksST --sort=no --language-force=Javascript";
        public override string Args { get { return _args; } }

        public override ITag Parse(string line)
        {
            string file, tagName, belongTo, signature;
            int lineNo;
            AccessType accessType;
            TagType tagType;
            if (_Parse(line, out tagName, out file, out lineNo, out tagType, out belongTo, out accessType, out signature))
                return new JavaScriptTag(file, lineNo, tagName, tagType, belongTo);
            else
                return null;
        }

        protected override TagType StrToTagType(string sTagType)
        {
            switch (sTagType)
            {
                case "c":
                    return TagType.JavaScript_Class;
                case "f":
                    return TagType.JavaScript_Function;
                case "m":
                    return TagType.JavaScript_Method;
                case "p":
                    return TagType.JavaScript_Property;
                case "v":
                    return TagType.JavaScript_GlobalVar;
                default:
                    throw new Exception(string.Format("Unknown JavaScript tag type '{0}'", sTagType));
            }
        }
    }
}