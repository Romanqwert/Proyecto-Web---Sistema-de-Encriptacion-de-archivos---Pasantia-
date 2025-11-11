namespace EncriptacionApi.Application.Services.Encryption
{
    public class XmlEncryptionService
    {
        private readonly EncryptHelper encryptHelper = new();

        public string EncryptXml(string xml, string key)
        {
            var doc = new System.Xml.XmlDocument();
            doc.LoadXml(xml);

            EncryptXmlNodes(doc.DocumentElement!, key);

            using var stringWriter = new StringWriter();
            using var xmlWriter = new System.Xml.XmlTextWriter(stringWriter)
            {
                Formatting = System.Xml.Formatting.Indented
            };
            doc.WriteTo(xmlWriter);
            xmlWriter.Flush();
            return stringWriter.ToString();
        }

        private void EncryptXmlNodes(System.Xml.XmlNode node, string key)
        {
            // Encripta el valor del nodo si es texto
            if (node.NodeType == System.Xml.XmlNodeType.Text ||
                node.NodeType == System.Xml.XmlNodeType.CDATA)
            {
                node.Value = encryptHelper.EncryptString(node.Value ?? string.Empty, key);
                return;
            }

            // Encripta atributos
            if (node.Attributes != null)
            {
                foreach (System.Xml.XmlAttribute attr in node.Attributes)
                {
                    if (attr.Name == "key" || attr.Name == "name") continue;
                    attr.Value = encryptHelper.EncryptString(attr.Value, key);
                }
            }

            // Llama recursivamente para los nodos hijos
            foreach (System.Xml.XmlNode child in node.ChildNodes)
            {
                EncryptXmlNodes(child, key);
            }
        }

        #region XML / CONFIG Decryption
        public string DecryptXml(string xml, string key)
        {
            var doc = new System.Xml.XmlDocument();
            doc.LoadXml(xml);
            DecryptXmlNodes(doc.DocumentElement!, key);

            using var stringWriter = new StringWriter();
            using var xmlWriter = new System.Xml.XmlTextWriter(stringWriter)
            {
                Formatting = System.Xml.Formatting.Indented
            };
            doc.WriteTo(xmlWriter);
            xmlWriter.Flush();
            return stringWriter.ToString();
        }

        private void DecryptXmlNodes(System.Xml.XmlNode node, string key)
        {
            // Desencripta valor del nodo si es texto
            if (node.NodeType == System.Xml.XmlNodeType.Text ||
                node.NodeType == System.Xml.XmlNodeType.CDATA)
            {
                node.Value = encryptHelper.TryDecryptString(node.Value ?? string.Empty, key);
                return;
            }

            // Desencripta atributos (menos los de "key" y "name")
            if (node.Attributes != null)
            {
                foreach (System.Xml.XmlAttribute attr in node.Attributes)
                {
                    if (attr.Name == "key" || attr.Name == "name") continue;
                    attr.Value = encryptHelper.TryDecryptString(attr.Value, key);
                }
            }

            // Recursión sobre nodos hijos
            foreach (System.Xml.XmlNode child in node.ChildNodes)
            {
                DecryptXmlNodes(child, key);
            }
        }

        #endregion

        // Encriptar XML solo en las keys especificadas
        public string EncryptXmlWithTargets(string xml, string key, List<string> encryptTargets)
        {
            var doc = new System.Xml.XmlDocument();
            doc.LoadXml(xml);

            EncryptXmlNodesWithTargets(doc.DocumentElement!, key, encryptTargets);

            using var stringWriter = new StringWriter();
            using var xmlWriter = new System.Xml.XmlTextWriter(stringWriter)
            {
                Formatting = System.Xml.Formatting.Indented
            };
            doc.WriteTo(xmlWriter);
            xmlWriter.Flush();
            return stringWriter.ToString();
        }

        private void EncryptXmlNodesWithTargets(System.Xml.XmlNode node, string key, List<string> encryptTargets, string currentPath = "")
        {
            string nodeName = node.Name;
            string fullPath = string.IsNullOrEmpty(currentPath) ? nodeName : $"{currentPath}.{nodeName}";

            // Encripta el valor del nodo si es texto y está en los targets
            if (node.NodeType == System.Xml.XmlNodeType.Text || node.NodeType == System.Xml.XmlNodeType.CDATA)
            {
                if (encryptHelper.ShouldEncryptKey(fullPath, encryptTargets))
                {
                    node.Value = encryptHelper.EncryptString(node.Value ?? string.Empty, key);
                }
                return;
            }

            // Encripta atributos si están en los targets
            if (node.Attributes != null)
            {
                foreach (System.Xml.XmlAttribute attr in node.Attributes)
                {
                    if (attr.Name == "key" || attr.Name == "name") continue;

                    string attrPath = $"{nodeName}.{attr.Name}";
                    if (encryptHelper.ShouldEncryptKey(attrPath, encryptTargets) || encryptHelper.ShouldEncryptKey(attr.Name, encryptTargets))
                    {
                        attr.Value = encryptHelper.EncryptString(attr.Value, key);
                    }
                }
            }

            // Llama recursivamente para los nodos hijos
            foreach (System.Xml.XmlNode child in node.ChildNodes)
            {
                EncryptXmlNodesWithTargets(child, key, encryptTargets, fullPath);
            }
        }
    }
}
