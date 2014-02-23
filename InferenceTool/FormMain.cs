using System;
using System.IO;
using System.Text;
using System.Windows.Forms;

namespace InferenceTool
{
	/// <summary>
	/// 
	/// </summary>
	public partial class FormMain : Form
	{
		private InferenceOutputDelegate infererOutputDelegate;

		/// <summary>
		/// Initializes a new instance of the <see cref="FormMain"/> class.
		/// </summary>
		public FormMain()
		{
			InitializeComponent();
			InitializeHandlers();

			infererOutputDelegate = new InferenceOutputDelegate(InfererComplete);
		}

		/// <summary>
		/// Handles the TextChanged event of the textBoxFile control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="T:System.EventArgs"/> instance containing the event data.</param>
		private void textBoxFile_TextChanged(object sender, EventArgs e)
		{
			string fileName = textBoxFile.Text;

			if (fileName.Length > 0)
			{
				schemaToolStripMenuItem.Enabled = File.Exists(fileName);
			}
			else
			{
				schemaToolStripMenuItem.Enabled = false;
			}
		}

		/// <summary>
		/// Handles the TextChanged event of the textBoxSchema control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="T:System.EventArgs"/> instance containing the event data.</param>
		private void textBoxSchema_TextChanged(object sender, EventArgs e)
		{
			saveToolStripMenuItem.Enabled = (textBoxSchema.Text.Length > 0);
		}

		/// <summary>
		/// Opens the file.
		/// </summary>
		private void OpenFile()
		{
			// Create the OpenFile Dialog
			using (OpenFileDialog openFile = new OpenFileDialog())
			{
				// Set the file filter to display xml files or all the files
				openFile.Filter = Properties.Resources.XmlFileFilter;
				// Show the dialog to the user and wait for the user decision
				if (openFile.ShowDialog(this) == DialogResult.OK)
				{
					// Set the file name
					textBoxFile.Text = openFile.FileName;
				}
			}
		}

		/// <summary>
		/// Saves the file.
		/// </summary>
		private void SaveFile()
		{
			using (SaveFileDialog saveFile = new SaveFileDialog())
			{
				saveFile.Filter = Properties.Resources.XsdFileFilter;
				if (saveFile.ShowDialog(this) == DialogResult.OK)
				{
					using (TextWriter writer = new StreamWriter(saveFile.FileName, false, Encoding.UTF8))
					{
						writer.Write(textBoxSchema.Text);
						writer.Flush();
						writer.Close();
					}
				}
			}
		}

		/// <summary>
		/// Infers the schema.
		/// </summary>
		private void InferSchema()
		{
			// Set user info
			toolStripStatusLabel.Text = Properties.Resources.WaitMessage;
			// Clear previous schema and errors
			textBoxSchema.Text = string.Empty;
			textBoxErrors.Text = string.Empty;

			// Create the Inference object
			Inference inference = new Inference(textBoxFile.Text);
			// Set the on complete listener
			inference.OnInferenceComplete += infererOutputDelegate;
			// Begin the inference process
			inference.Begin();
		}

		/// <summary>
		/// Inferense is complete.
		/// </summary>
		/// <param name="schema">The schema.</param>
		/// <param name="errors">The errors.</param>
		private void InfererComplete(string schema, string errors)
		{
			// Check if the control is invoked in the correct thread
			if (textBoxSchema.InvokeRequired)
			{
				// Invoke the method in the correct thread
				Invoke(infererOutputDelegate, new object[] {schema, errors});
			}
			// Set the properties
			else
			{
				textBoxSchema.Text = schema;
				textBoxErrors.Text = errors;
				toolStripStatusLabel.Text = string.Empty;
			}
		}
	}
}