using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Xml;
using System.Xml.Schema;

namespace InferenceTool
{
	/// <summary>
	/// 
	/// </summary>
	internal class Inference
	{
		private InferenceOutputDelegate m_infererOutputDelegate;
		private string m_fileName;

		/// <summary>
		/// Initializes a new instance of the <see cref="Inference"/> class.
		/// </summary>
		internal Inference()
		{
			m_fileName = null;
			m_infererOutputDelegate = null;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="Inference"/> class.
		/// </summary>
		/// <param name="fileName">Name of the file.</param>
		internal Inference(string fileName) : this()
		{
			m_fileName = fileName;
		}

		/// <summary>
		/// Gets or sets the inferer delegate.
		/// </summary>
		/// <value>The inferer delegate.</value>
		internal event InferenceOutputDelegate OnInferenceComplete
		{
			add { m_infererOutputDelegate += value; }
			remove { m_infererOutputDelegate -= value; }
		}

		/// <summary>
		/// Begins this instance.
		/// </summary>
		internal void Begin()
		{
			if (m_fileName != null)
			{
				Thread thread = new Thread(new ThreadStart(InferSchema));
				thread.Start();
			}
		}

		/// <summary>
		/// Begins the specified file name.
		/// </summary>
		/// <param name="fileName">Name of the file.</param>
		internal void Begin(string fileName)
		{
			m_fileName = fileName;
			Begin();
		}

		/// <summary>
		/// Infers the schema.
		/// </summary>
		private void InferSchema()
		{
			string schemaText = string.Empty;
			string schemaError = string.Empty;

			XmlSchemaInference infer = new XmlSchemaInference();
			XmlTextReader xmlReader = new XmlTextReader(m_fileName);
			XmlSchemaSet schemas = null;

			try
			{
				schemas = infer.InferSchema(xmlReader);
			}
			catch (Exception exception)
			{
				StringBuilder builder = new StringBuilder();
				builder.AppendLine(exception.Message);
				builder.AppendLine(exception.StackTrace);
				schemaError = builder.ToString();
			}
			finally
			{
				xmlReader.Close();
			}

			if (schemas != null)
			{
				schemaText = GetSchemaString(schemas);
			}

			if (m_infererOutputDelegate != null)
			{
				m_infererOutputDelegate(schemaText, schemaError);
			}
		}

		/// <summary>
		/// Gets the schema string.
		/// </summary>
		/// <param name="schemas">The schemas.</param>
		/// <returns></returns>
		private static string GetSchemaString(XmlSchemaSet schemas)
		{
			lock(typeof(Inference))
			{
				string schemaText;

				using (MemoryStream stream = new MemoryStream())
				{
					using (TextWriter writer = new StreamWriter(stream))
					{
						foreach (XmlSchema schema in schemas.Schemas())
						{
							string targetNamespace = schema.TargetNamespace;
							if (!string.IsNullOrEmpty(targetNamespace))
							{
								writer.WriteLine(targetNamespace);
							}
							schema.Write(writer);

							foreach (XmlSchemaImport xsi in schema.Includes)
							{
								string xsiNamespace = xsi.Namespace;
								if (!string.IsNullOrEmpty(xsiNamespace))
								{
									writer.WriteLine(xsiNamespace);
								}
								xsi.Schema.Write(writer);
							}
						}

						writer.Flush();
						stream.Flush();

						using (TextReader reader = new StreamReader(stream))
						{
							stream.Position = 0;
							schemaText = reader.ReadToEnd();
							reader.Close();
						}

						stream.Close();
						writer.Close();
					}
				}

				return schemaText;
			}
		}
	}
}