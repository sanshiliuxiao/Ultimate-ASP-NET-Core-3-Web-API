using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Dynamic;
using System.Text;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;

namespace Entities.Models
{
    // 为了让 Data shaping  更好看，需要继承一些 类 和 接口
    public class Entity : DynamicObject, IXmlSerializable, IDictionary<string, object>
    {
        private readonly string _root = nameof(Entity);
        private readonly IDictionary<string, object> _expando = null;

        public Entity()
        {
            _expando = new ExpandoObject();
        }

        public object this[string key] { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public ICollection<string> Keys => throw new NotImplementedException();

        public ICollection<object> Values => throw new NotImplementedException();

        public int Count
        {
            get
            {
                return _expando.Count;
            }
        }

        public bool IsReadOnly
        {
            get
            {
                return _expando.IsReadOnly;
            }
        }
        public void Add(string key, object value)
        {
            _expando.Add(key, value);
        }

        public void Add(KeyValuePair<string, object> item)
        {
            _expando.Add(item);
        }

        public void Clear()
        {
            _expando.Clear();
        }

        public bool Contains(KeyValuePair<string, object> item)
        {
            return _expando.Contains(item);
        }

        public bool ContainsKey(string key)
        {
            return _expando.ContainsKey(key);
        }

        public void CopyTo(KeyValuePair<string, object>[] array, int arrayIndex)
        {
            _expando.CopyTo(array, arrayIndex);
        }

        public IEnumerator<KeyValuePair<string, object>> GetEnumerator()
        {
            return _expando.GetEnumerator();
        }

        public XmlSchema GetSchema()
        {
            throw new NotImplementedException();
        }

        public void ReadXml(XmlReader reader)
        {
            reader.ReadStartElement(_root);
            while (!reader.Name.Equals(_root))
            {
                string typeCountent;
                Type underlyingType;
                var name = reader.Name;

                reader.MoveToAttribute("type");
                typeCountent = reader.ReadContentAsString();
                underlyingType = Type.GetType(typeCountent); ;

                reader.MoveToContent();
                _expando[name] = reader.ReadElementContentAs(underlyingType, null);
            }
        }

        public bool Remove(string key)
        {
            return _expando.Remove(key);
        }

        public bool Remove(KeyValuePair<string, object> item)
        {
            return _expando.Remove(item);
        }


        public override bool TryGetMember(GetMemberBinder binder, out object result)
        {
            if (_expando.TryGetValue(binder.Name, out object value))
            {
                result = value;
                return true;
            }
            return base.TryGetMember(binder, out result);
        }
        public bool TryGetValue(string key, [MaybeNullWhen(false)] out object value)
        {
            return _expando.TryGetValue(key, out value);
        }

        public override bool TrySetMember(SetMemberBinder binder, object value)
        {
            _expando[binder.Name] = value;
            return true;
        }

        public void WriteXml(XmlWriter writer)
        {
            foreach(var key in _expando.Keys)
            {
                var value = _expando[key];
                WriteLinksToXml(key, value, writer);
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        private void WriteLinksToXml(string key, object value, XmlWriter writer)
        {
            writer.WriteStartElement(key);
            writer.WriteString(value.ToString());
            writer.WriteEndElement();
        }
    }
}
