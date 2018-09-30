using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.IO;
using System.Drawing;

namespace smartEdit.Core {
    public class SerializerBase {
        public SerializerBase(string SerializerDocName, string SerializerVersion) {
            m_DataSerializerName = SerializerDocName;
            m_DataSerializerVersion = SerializerVersion;
        }
        public virtual void CloseInputStream() { throw new NotImplementedException("CloseInputStream"); }
        public virtual void CloseOutputStream() { }
        public virtual void OpenInputStream() { }
        public virtual void OpenOutputStream() { }
        /// <summary>
        /// This indicates if the stream is available for writing.
        /// Even after opening the stream, this might return false because the device is busy. 
        /// </summary>
        /// <returns></returns>
        public virtual bool CanWrite() { return true; }
        /// <summary>
        /// This indicates if the stream is available for reading.
        /// Even after opening the stream, this might return false because the device is busy. 
        /// </summary>
        /// <returns></returns>
        public virtual bool CanRead() { return true; }
        #region Writing
        /// <summary>
        /// Starts a section. A section represents a piece of complex data (f.e. an object).
        /// After starting a section you can WriteData to push the attributes of the object on the stream.
        /// Its possible to nest sections.
        /// You have to use WriteElementEnd to indicate that you have finished on the object.
        /// </summary>
        /// <param name="Name"></param>
        public virtual void WriteElementStart(string Name) { }
        /// <summary>
        /// See WriteStartElement
        /// </summary>
        /// <param name="Name"></param>
        public virtual void WriteElementEnd(string Name) { }
        public virtual void WriteData(string Name, Point Data) { }
        public virtual void WriteData(string Name, Rectangle Data) { }
        public virtual void WriteData(string Name, Size Data) { }
        public virtual void WriteData(string Name, string Text) { }
        public virtual void WriteData(string Name, byte[] Data) { }
        public virtual void WriteData(string Name, int Data) { }
        /// <summary>
        /// Writes the header of the stream, that can contain additional data for proessing the stream.
        /// It should at least contain the name of the serializer and the version.
        /// This is called after opening the stream.
        /// </summary>
        protected virtual void WriteStreamHeader() { }
        /// <summary>
        /// Can be overloaded to append additional data at the end of the stream (f.e. a checksum).
        /// This is called before Closing the stream.
        /// </summary>
        protected virtual void WriteStreamTail() { }
        /// <summary>
        /// Should be overloaded to extract the Header data (name of the serializer and the version).
        /// </summary>
        /// <returns></returns>
        protected virtual bool ReadStreamHeader() { return false; }
        /// <summary>
        /// Currently not used.
        /// </summary>
        /// <returns></returns>
        protected virtual bool ReadStreamTail() { return false; }
        #endregion
        public enum NodeType {
            None = 0,
            NodeStart,
            NodeEnd,
            Attribute
        }
        #region Reading
        /// <summary>
        /// Read to the next piece of data in the stream. If there is no more data, it returns false;
        /// </summary>
        /// <returns></returns>
        public virtual bool ReadNext() { return false; }
        public virtual int GetNodeLevel() { return 0; }
        public virtual NodeType GetNodeType() { return m_CurrentNodeType; }
        /// <summary>
        /// Returns the Document Serializer Name found in Stream. 
        /// Check this value in your application for validity.
        /// This value is available after opening the input stream. An exception will be triggered if this value is not found in stream.
        /// </summary>
        public string GetDetectedSerializerName() { return m_FoundDocNameText; }
        /// <summary>
        /// Returns the Document Serializer Version found in Stream. 
        /// Check this value in your application for validity.
        /// This value is available after opening the input stream. An exception will be triggered if this value is not found in stream.
        /// </summary>
        public string GetDetectedDocumentVersion() { return m_FoundDocVersionText; }
        /// <summary>
        /// Returns the Document Serializer Name that is required. 
        /// </summary>
        /// <returns></returns>
        public string GetRequiredSerializerName() { return m_DataSerializerName; }
        /// <summary>
        /// Returns the Document Serializer Version that is required. 
        /// </summary>
        /// <returns></returns>
        public string GetRequiredDocumentVersion() { return m_DataSerializerVersion; }
        /// <summary>
        /// Gets the name of the current section/ attribute.
        /// </summary>
        /// <returns></returns>
        public virtual string GetNodeName() { return string.Empty; }
        //public virtual NodeType GetNodeType() { return m_CurrentNodeType; }
        public virtual Point ReadAsPoint(string Name) { return Point.Empty; }
        public virtual Rectangle ReadAsRect(string Name) { return Rectangle.Empty; }
        public virtual Size ReadAsSize(string Name) { return Size.Empty; }
        public virtual string ReadAsString(string Name) { return string.Empty; }
        public virtual byte[] ReadAsBinary(string Name) { return null; }
        public virtual int ReadAsInt(string Name) { return 0; }
        #endregion

        protected string m_DataSerializerName = "";
        protected string m_DataSerializerVersion = "0.0.0.0";
        /// <summary>
        /// The Serializer document version. Set this value in ReadStreamHeader.
        /// </summary>
        protected string m_FoundDocVersionText = "";
        /// <summary>
        /// The Serializer document name. Set this value in ReadStreamHeader.
        /// </summary>
        protected string m_FoundDocNameText = "";
        protected NodeType m_CurrentNodeType = NodeType.None;
    }

    public class SerializerXML : SerializerBase {
        public enum FieldName {
            Document,
            SerializerDocName,
            SerializerVersion,
            UserData
        }
        public SerializerXML(string SerializerDocName, string SerializerVersion)
            : base(SerializerDocName, SerializerVersion) {
            m_Writer = null;
            m_Reader = null;
        }
        public override void OpenOutputStream() {
            base.OpenOutputStream();
        }
        public override void OpenInputStream() {
            base.OpenInputStream();
        }
        public void OpenOutputStream(Stream OutStream) {
            m_Stream = OutStream;
            XmlWriterSettings _Settings = new XmlWriterSettings();
            _Settings.Indent = true;
            _Settings.IndentChars = " ";
            _Settings.NewLineChars = "\r\n";
            _Settings.NewLineHandling = NewLineHandling.None;
            //_Settings.OutputMethod = XmlOutputMethod.Xml;
            _Settings.CloseOutput = true;
            if (m_Writer != null) {
                m_Writer.Close();
            }
            m_Writer = XmlWriter.Create(m_Stream, _Settings);
            WriteStreamHeader();
        }
        public void OpenOutputStream(string FileName) {
            m_Stream = new FileStream(FileName, FileMode.Create, FileAccess.Write, FileShare.None);
            XmlWriterSettings _Settings = new XmlWriterSettings();
            _Settings.Indent = true;
            _Settings.IndentChars = " ";
            _Settings.NewLineChars = "\r\n";
            _Settings.NewLineHandling = NewLineHandling.None;
            //_Settings.OutputMethod = XmlOutputMethod.Xml;
            _Settings.CloseOutput = true;
            if (m_Writer != null) {
                m_Writer.Close();
            }
            m_Writer = XmlWriter.Create(m_Stream, _Settings);
            WriteStreamHeader();
        }
        public void OpenInputStream(Stream InStream) {
            m_Stream = InStream;
            XmlReaderSettings _Settings = new XmlReaderSettings();
            _Settings.IgnoreComments = true;
            _Settings.CloseInput = true;
            _Settings.ProhibitDtd = false; //??! else DTD exception
            if (m_Reader != null) {
                m_Reader.Close();
            }
            m_Reader = XmlReader.Create(m_Stream, _Settings);
            m_Reader.Read();
            do {
                if (GetNodeType() != Core.SerializerBase.NodeType.NodeEnd) {
                    ReadStreamHeader();
                }
            } while (ReadNext() && (m_FoundDocNameText == "" || m_FoundDocVersionText == ""));

            if ((m_FoundDocNameText == "") || (m_FoundDocVersionText == "")) {
                throw new FormatException("DocName and/or DocVersion not found");
            };
        }
        public void OpenInputStream(string FileName) {
            m_Stream = new FileStream(FileName, FileMode.Open, FileAccess.Read, FileShare.Read);
            XmlReaderSettings _Settings = new XmlReaderSettings();
            _Settings.IgnoreComments = true;
            _Settings.CloseInput = true;
            _Settings.ProhibitDtd = false; //??! else DTD exception
            if (m_Reader != null) {
                m_Reader.Close();
            }
            m_Reader = XmlReader.Create(m_Stream, _Settings);
            m_Reader.Read();
            do {
                if (GetNodeType() != Core.SerializerBase.NodeType.NodeEnd) {
                    ReadStreamHeader();
                }
            } while (ReadNext() && (m_FoundDocNameText == "" || m_FoundDocVersionText == ""));

            if ((m_FoundDocNameText == "") || (m_FoundDocVersionText == "")) {
                throw new FormatException("DocName and/or DocVersion not found");
            };
        }
        public override void CloseOutputStream() {
            if (m_Writer != null) {
                WriteStreamTail();
                m_Writer.Close();
            }
        }
        public override void CloseInputStream() {
            if (m_Reader != null) {
                m_Reader.Close();
            }
        }
        #region Writing
        public override void WriteElementStart(string Name) {
            m_Writer.WriteStartElement(Name);
        }
        public override void WriteElementEnd(string Name) {
            m_Writer.WriteEndElement();
            m_Writer.Flush();
        }
        public override void WriteData(string Name, Point Data) {
            m_Writer.WriteAttributeString(Name, string.Format("{0},{1}", Data.X, Data.Y));
        }
        public override void WriteData(string Name, Rectangle Data) {
            m_Writer.WriteAttributeString(Name, string.Format("{0},{1},{2},{3}", Data.X, Data.Y, Data.Width, Data.Height));
        }
        public override void WriteData(string Name, Size Data) {
            m_Writer.WriteAttributeString(Name, string.Format("{0},{1}", Data.Width, Data.Height));
        }
        public override void WriteData(string Name, string Text) {
            m_Writer.WriteAttributeString(Name, Text);
        }
        public override void WriteData(string Name, byte[] Data) {
            m_Writer.WriteStartAttribute(Name);
            m_Writer.WriteBinHex(Data, 0, Data.Length);
            m_Writer.WriteEndAttribute();
        }
        public override void WriteData(string Name, int Data) {
            m_Writer.WriteAttributeString(Name, Data.ToString());
        }
        protected override void WriteStreamHeader() {
            m_Writer.WriteStartDocument();
            //m_Writer.WriteDocType(m_DataSerializerName, null, null, null);
            m_Writer.WriteStartElement(FieldName.Document.ToString());
            m_Writer.WriteAttributeString(FieldName.SerializerDocName.ToString(), m_DataSerializerName);
            m_Writer.WriteAttributeString(FieldName.SerializerVersion.ToString(), m_DataSerializerVersion);

        }
        protected override void WriteStreamTail() {
            m_Writer.WriteEndElement();
            m_Writer.WriteEndDocument();
        }

        #endregion
        #region Reading
        public override int GetNodeLevel() {
            if (m_Reader != null) {
                return m_Reader.Depth;
            } else { return 0; }
        }
        public override bool ReadNext() {
            bool Continue = false, Stop = false;
            do {   //check if there is another attribute
                Continue = m_Reader.MoveToNextAttribute();
                m_CurrentNodeType = NodeType.Attribute;
                //else find next node start
                if (!Continue) {
                    Stop = !m_Reader.Read();
                    switch (m_Reader.NodeType) {
                        case XmlNodeType.Comment:
                        case XmlNodeType.None:
                        case XmlNodeType.Whitespace:
                        case XmlNodeType.EndEntity:
                            Continue = false;
                            m_CurrentNodeType = NodeType.None;
                            break;
                        case XmlNodeType.EndElement:
                            Continue = true;
                            m_CurrentNodeType = NodeType.NodeEnd;
                            break;
                        default:
                            Continue = true;
                            m_CurrentNodeType = NodeType.NodeStart;
                            break;
                    }
                    //Continue = (m_Reader.NodeType != XmlNodeType.None); 
                }
            } while (!(Continue || Stop));
            return Continue;
        }
        protected override bool ReadStreamHeader() {
            bool FoundHeader = false;
            //if (GetNodeType() != NodeType.Attribute) return FoundHeader;
            if (GetNodeName() == SerializerXML.FieldName.SerializerDocName.ToString()) {
                FoundHeader = true;
                m_FoundDocNameText = ReadAsString(GetNodeName());
            } else if (GetNodeName() == SerializerXML.FieldName.SerializerVersion.ToString()) {
                FoundHeader = true;
                m_FoundDocVersionText = ReadAsString(GetNodeName());
            }
            return FoundHeader;
        }
        public override string GetNodeName() {
            return m_Reader.Name;
        }
        public override Point ReadAsPoint(string Name) {
            if (!m_Reader.MoveToAttribute(Name)) return Point.Empty;
            string[] Text = m_Reader.ReadContentAsString().Split(',');
            Point Obj = new Point(Convert.ToInt32(Text[0]),
                Convert.ToInt32(Text[1]));
            return Obj; ;
        }
        public override Rectangle ReadAsRect(string Name) {
            if (!m_Reader.MoveToAttribute(Name)) return Rectangle.Empty;
            string[] Text = m_Reader.ReadContentAsString().Split(',');
            Rectangle Obj = new Rectangle(Convert.ToInt32(Text[0]),
                Convert.ToInt32(Text[1]),
                Convert.ToInt32(Text[2]),
                Convert.ToInt32(Text[3]));
            return Obj;
        }
        public override Size ReadAsSize(string Name) {
            if (!m_Reader.MoveToAttribute(Name)) return Size.Empty;
            string[] Text = m_Reader.ReadContentAsString().Split(',');
            Size Obj = new Size(Convert.ToInt32(Text[0]),
                Convert.ToInt32(Text[1]));
            return Obj;
        }
        public override string ReadAsString(string Name) {
            if (!m_Reader.MoveToAttribute(Name)) return string.Empty;
            return m_Reader.ReadContentAsString();
        }
        public override byte[] ReadAsBinary(string Name) {
            if (!m_Reader.MoveToAttribute(Name)) return null;
            /*byte[] _Buffer = new byte[0];
            int _Size=0, _Left=0;
            do
            {
                _Left= m_Reader.ReadContentAsBinHex(_Buffer, _Size, 256);
                _Size += _Left;
            } while (_Left > 0);

            return _Buffer; */
            return null;
        }
        public override int ReadAsInt(string Name) {
            if (!m_Reader.MoveToAttribute(Name)) return 0;
            return m_Reader.ReadContentAsInt();
        }

        #endregion

        protected Stream m_Stream = null;
        protected XmlReader m_Reader = null;
        protected XmlWriter m_Writer = null;

    }
    /// <summary>
    /// Some tool to export to bitmap
    /// </summary>
    public class SerializerBMP : SerializerBase {
        public SerializerBMP(string SerializerDocName, string SerializerVersion)
            : base(SerializerDocName, SerializerVersion) {
            m_BMP = new Bitmap(200, 200); //??
            Graphics m_Graphic = Graphics.FromImage(m_BMP);
        }
        public override void OpenOutputStream() {
        }
        public void OpenOutputStream(ModelDiagram Model, string FileName, Rectangle PageSize) {
            m_Model = Model;
            m_BMP = new Bitmap(PageSize.Width, PageSize.Height);
            m_Graphic = Graphics.FromImage(m_BMP);
            m_Stream = new FileStream(FileName, FileMode.Create, FileAccess.Write, FileShare.None);
            DrawShapes();
        }
        private void DrawShapes() {
            if (m_Model == null) return;
            ShapeDrawingContext Context = new ShapeDrawingContext();
            Context.SetScale(1);
            ElementEnumerator<ShapeInterface> Iterator = m_Model.GetShapeEnumerator();
            //fill background
            m_Graphic.FillRectangle(Brushes.White, m_Graphic.ClipBounds);
            while (Iterator.MoveNext()) {
                Iterator.Current.Draw(m_Graphic, Context);
            }
        }
        public override void CloseOutputStream() {
            m_BMP.Save(m_Stream, System.Drawing.Imaging.ImageFormat.Bmp);
            /*?? if (m_Writer != null)
             {
                 WriteStreamTail();
                 m_Writer.Close();
             }*/
            m_BMP.Dispose();
            //m_Graphic.Dispose();
            m_Stream.Dispose();
        }
        ModelDiagram m_Model = null;
        Bitmap m_BMP = null;
        Graphics m_Graphic = null;
        Stream m_Stream = null;
    }
}
