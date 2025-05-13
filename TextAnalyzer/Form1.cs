using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO; // Додано для роботи з файлами
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TextAnalyzer
{
    public partial class Form1: Form
    {
		private TextBox inputTextBox;
		private TextBox resultTextBox;
		private Button analyzeButton;
		private Button clearButton; // Додаємо кнопку очищення
		private Panel resultPanel;
		private Label titleLabel;
		private Label inputLabel;
		private Label resultLabel;
		private MenuStrip mainMenu;
		// Кольори для тем
		private readonly ColorScheme lightTheme = new ColorScheme
		{
			Background = Color.WhiteSmoke,
			Text = Color.Black,
			ControlBack = Color.White,
			ButtonBack = Color.MediumSlateBlue,
			ButtonText = Color.White,
			ClearButtonBack = Color.LightCoral,
			PanelBack = Color.White,
			PanelBorder = Color.Gray
		};

		private readonly ColorScheme darkTheme = new ColorScheme
		{
			Background = Color.FromArgb(45, 45, 48),
			Text = Color.White,
			ControlBack = Color.FromArgb(30, 30, 30),
			ButtonBack = Color.FromArgb(0, 122, 204),
			ButtonText = Color.White,
			ClearButtonBack = Color.FromArgb(200, 80, 80),
			PanelBack = Color.FromArgb(30, 30, 30),
			PanelBorder = Color.FromArgb(70, 70, 70)
		};

		private ColorScheme currentTheme;

		private class ColorScheme
		{
			public Color Background { get; set; }
			public Color Text { get; set; }
			public Color ControlBack { get; set; }
			public Color ButtonBack { get; set; }
			public Color ButtonText { get; set; }
			public Color ClearButtonBack { get; set; }
			public Color PanelBack { get; set; }
			public Color PanelBorder { get; set; }
		}


		// Для адаптивного дизайну
		private Size originalFormSize;
		private Dictionary<Control, Rectangle> controlOriginalRects = new Dictionary<Control, Rectangle>();

		public Form1()
        {
			InitializeComponent(); // Видаліть цей рядок, якщо ви не використовуєте дизайнер форм
			currentTheme = lightTheme;
			SetupForm(); // Перейменовано з InitializeForm
			SetupMenu(); // Перейменовано з InitializeMenu
			this.Resize += Form1_Resize;
			ApplyTheme();
		}

		private void SetupForm()
		{
			this.Text = "🧠 Аналізатор змістовності тексту";
			this.Size = new Size(850, 650);
			this.MinimumSize = new Size(600, 400);
			this.StartPosition = FormStartPosition.CenterScreen;

			// Заголовок
			titleLabel = new Label
			{
				Text = "Аналізатор тексту",
				Font = new Font("Segoe UI", 16, FontStyle.Bold),
				AutoSize = true,
				Location = new Point(20, 55)
			};
			this.Controls.Add(titleLabel);

			// Мітка введення тексту
			inputLabel = new Label
			{
				Text = "Введіть текст для аналізу:",
				AutoSize = true,
				Location = new Point(20, 100)
			};
			this.Controls.Add(inputLabel);

			// Поле введення тексту
			inputTextBox = new TextBox
			{
				Multiline = true,
				ScrollBars = ScrollBars.Vertical,
				Font = new Font("Segoe UI", 10),
				Location = new Point(20, 130),
				Size = new Size(780, 180)
			};
			this.Controls.Add(inputTextBox);

			// Кнопка аналізу
			analyzeButton = new Button
			{
				Text = "🔍 Аналізувати текст",
				FlatStyle = FlatStyle.Flat,
				Font = new Font("Segoe UI", 10, FontStyle.Bold),
				Location = new Point(20, 330),
				Size = new Size(180, 40)
			};
			analyzeButton.FlatAppearance.BorderSize = 0;
			analyzeButton.Click += (sender, e) =>
			{
				if (string.IsNullOrWhiteSpace(inputTextBox.Text))
				{
					MessageBox.Show("Будь ласка, введіть текст для аналізу.", "Помилка",
						MessageBoxButtons.OK, MessageBoxIcon.Warning);
					return;
				}

				var analysisResults = AnalyzeText(inputTextBox.Text);
				resultTextBox.Text = FormatAnalysisResults(analysisResults);
			};
			this.Controls.Add(analyzeButton);

			// Кнопка очищення
			clearButton = new Button
			{
				Text = "Очистити",
				FlatStyle = FlatStyle.Flat,
				Font = new Font("Segoe UI", 10, FontStyle.Bold),
				Location = new Point(analyzeButton.Right + 20, 330),
				Size = new Size(180, 40)
			};
			clearButton.FlatAppearance.BorderSize = 0;
			clearButton.Click += (sender, e) =>
			{
				inputTextBox.Clear();
				resultTextBox.Clear();
			};
			this.Controls.Add(clearButton);

			// Мітка результатів
			resultLabel = new Label
			{
				Text = "Результати аналізу:",
				AutoSize = true,
				Location = new Point(20, 390)
			};
			this.Controls.Add(resultLabel);

			// Панель результатів
			resultPanel = new Panel
			{
				Location = new Point(20, 420),
				Size = new Size(780, 180)
			};

			resultTextBox = new TextBox
			{
				Multiline = true,
				ScrollBars = ScrollBars.Vertical,
				Dock = DockStyle.Fill,
				ReadOnly = true,
				BorderStyle = BorderStyle.None,
				Font = new Font("Consolas", 9.5f)
			};

			resultPanel.Controls.Add(resultTextBox);
			this.Controls.Add(resultPanel);
		}

		private void SetupMenu()
		{
			mainMenu = new MenuStrip
			{
				BackColor = currentTheme.ControlBack,
				ForeColor = currentTheme.Text
			};

			// Меню "Файл"
			var fileMenu = new ToolStripMenuItem("Файл");
			var openItem = new ToolStripMenuItem("Відкрити", null, OpenFile);
			var saveItem = new ToolStripMenuItem("Зберегти", null, SaveFile);
			var exitItem = new ToolStripMenuItem("Вийти", null, ExitApp);
			fileMenu.DropDownItems.AddRange(new ToolStripItem[] { openItem, saveItem, new ToolStripSeparator(), exitItem });

			// Меню "Тема"
			var themeMenu = new ToolStripMenuItem("Тема");
			var lightThemeItem = new ToolStripMenuItem("Світла", null, (s, e) => SetTheme(lightTheme));
			var darkThemeItem = new ToolStripMenuItem("Темна", null, (s, e) => SetTheme(darkTheme));
			themeMenu.DropDownItems.AddRange(new ToolStripItem[] { lightThemeItem, darkThemeItem });

			// Меню "Допомога"
			var helpMenu = new ToolStripMenuItem("Допомога");
			var aboutItem = new ToolStripMenuItem("Про програму", null, ShowAbout);
			helpMenu.DropDownItems.Add(aboutItem);

			mainMenu.Items.AddRange(new ToolStripItem[] { fileMenu, themeMenu, helpMenu });
			this.Controls.Add(mainMenu);
			this.MainMenuStrip = mainMenu;
		}

		private void SetTheme(ColorScheme theme)
		{
			currentTheme = theme;
			ApplyTheme();
		}

		private void ApplyTheme()
		{
			this.BackColor = currentTheme.Background;
			this.ForeColor = currentTheme.Text;

			foreach (Control control in this.Controls)
			{
				ApplyThemeToControl(control);
			}

			if (mainMenu != null)
			{
				mainMenu.BackColor = currentTheme.ControlBack;
				mainMenu.ForeColor = currentTheme.Text;
			}
		}

		private void ApplyThemeToControl(Control control)
		{
			control.BackColor = currentTheme.ControlBack;
			control.ForeColor = currentTheme.Text;

			if (control is TextBox textBox)
			{
				textBox.BackColor = currentTheme.ControlBack;
				textBox.ForeColor = currentTheme.Text;
				textBox.BorderStyle = control is Panel ? BorderStyle.None : BorderStyle.FixedSingle;
			}
			else if (control is Button button)
			{
				if (button.Text == "Очистити")
				{
					button.BackColor = currentTheme.ClearButtonBack;
				}
				else
				{
					button.BackColor = currentTheme.ButtonBack;
				}
				button.ForeColor = currentTheme.ButtonText;
			}
			else if (control is Panel panel)
			{
				panel.BackColor = currentTheme.PanelBack;
				panel.BorderStyle = BorderStyle.FixedSingle;
			}

			foreach (Control childControl in control.Controls)
			{
				ApplyThemeToControl(childControl);
			}
		}

		private void Form1_Resize(object sender, EventArgs e)
		{
			int margin = 20;
			int menuHeight = mainMenu != null ? mainMenu.Height : 0;

			inputTextBox.Width = this.ClientSize.Width - 2 * margin;
			resultPanel.Width = this.ClientSize.Width - 2 * margin;
			resultPanel.Height = this.ClientSize.Height - resultPanel.Top - margin;

			clearButton.Location = new Point(analyzeButton.Right + 20, analyzeButton.Top);
		}

		private void OpenFile(object sender, EventArgs e)
		{
			using (OpenFileDialog openFileDialog = new OpenFileDialog())
			{
				openFileDialog.Filter = "Текстові файли (*.txt)|*.txt|Усі файли (*.*)|*.*";

				if (openFileDialog.ShowDialog() == DialogResult.OK)
				{
					try
					{
						inputTextBox.Text = File.ReadAllText(openFileDialog.FileName);
					}
					catch (Exception ex)
					{
						MessageBox.Show($"Помилка при відкритті файлу: {ex.Message}", "Помилка",
							MessageBoxButtons.OK, MessageBoxIcon.Error);
					}
				}
			}
		}

		private void SaveFile(object sender, EventArgs e)
		{
			using (SaveFileDialog saveFileDialog = new SaveFileDialog())
			{
				saveFileDialog.Filter = "Текстові файли (*.txt)|*.txt|Усі файли (*.*)|*.*";

				if (saveFileDialog.ShowDialog() == DialogResult.OK)
				{
					try
					{
						File.WriteAllText(saveFileDialog.FileName, resultTextBox.Text);
						MessageBox.Show("Файл успішно збережено!", "Успіх",
							MessageBoxButtons.OK, MessageBoxIcon.Information);
					}
					catch (Exception ex)
					{
						MessageBox.Show($"Помилка при збереженні файлу: {ex.Message}", "Помилка",
							MessageBoxButtons.OK, MessageBoxIcon.Error);
					}
				}
			}
		}

		private void ExitApp(object sender, EventArgs e)
		{
			Application.Exit();
		}

		private void ShowAbout(object sender, EventArgs e)
		{
			MessageBox.Show("Аналізатор тексту\nВерсія 1.0\nРозробник: Єгор Запорожан\n\nЦя програма аналізує текст за різними параметрами.",
			  "Про програму",
			  MessageBoxButtons.OK,
			  MessageBoxIcon.Information);
		}

		private Dictionary<string, object> AnalyzeText(string text)
		{
			var results = new Dictionary<string, object>();

			results["Length"] = text.Length;
			results["WordCount"] = CountWords(text);
			results["SentenceCount"] = CountSentences(text);
			results["ParagraphCount"] = CountParagraphs(text);
			results["AvgWordLength"] = CalculateAverageWordLength(text);
			results["AvgSentenceLength"] = CalculateAverageSentenceLength(text);
			results["UniqueWords"] = CountUniqueWords(text);
			results["LexicalDiversity"] = CalculateLexicalDiversity(text);
			results["ReadabilityScore"] = CalculateReadabilityScore(text);
			results["KeywordDensity"] = CalculateKeywordDensity(text, 5);

			return results;
		}

		private int CountWords(string text)
		{
			if (string.IsNullOrWhiteSpace(text))
				return 0;

			return Regex.Matches(text, @"\b[\w']+\b").Count;
		}

		private int CountSentences(string text)
		{
			if (string.IsNullOrWhiteSpace(text))
				return 0;

			return text.Split(new[] { '.', '!', '?', ';', ':', '\n' }, StringSplitOptions.RemoveEmptyEntries).Length;
		}

		private int CountParagraphs(string text)
		{
			if (string.IsNullOrWhiteSpace(text))
				return 0;

			return text.Split(new[] { "\r\n\r\n", "\n\n" }, StringSplitOptions.RemoveEmptyEntries).Length;
		}

		private double CalculateAverageWordLength(string text)
		{
			var words = Regex.Matches(text, @"\b[\w']+\b").Cast<Match>().Select(m => m.Value);
			return words.Any() ? words.Average(w => w.Length) : 0;
		}

		private double CalculateAverageSentenceLength(string text)
		{
			var sentences = text.Split(new[] { '.', '!', '?', ';', ':', '\n' }, StringSplitOptions.RemoveEmptyEntries);
			return sentences.Any() ? sentences.Average(s => CountWords(s)) : 0;
		}

		private int CountUniqueWords(string text)
		{
			return Regex.Matches(text.ToLower(), @"\b[\w']+\b")
				.Cast<Match>()
				.Select(m => m.Value)
				.Distinct()
				.Count();
		}

		private double CalculateLexicalDiversity(string text)
		{
			var wordCount = CountWords(text);
			return wordCount == 0 ? 0 : (double)CountUniqueWords(text) / wordCount;
		}

		private double CalculateReadabilityScore(string text)
		{
			var words = CountWords(text);
			var sentences = CountSentences(text);
			return words == 0 || sentences == 0 ? 0 :
				206.835 - 1.015 * ((double)words / sentences) - 84.6 * CalculateAverageSyllablesPerWord(text);
		}

		private double CalculateAverageSyllablesPerWord(string text)
		{
			var words = Regex.Matches(text, @"\b[\w']+\b").Cast<Match>().Select(m => m.Value);
			return words.Any() ? (double)words.Sum(w => CountSyllables(w)) / words.Count() : 0;
		}

		private int CountSyllables(string word)
		{
			word = word.ToLower().Trim();
			if (word.Length < 1) return 0;

			int count = 0;
			bool prevWasVowel = false;

			foreach (char c in word)
			{
				bool isVowel = "аеєиіїоуюяыэ".IndexOf(c) >= 0;
				if (isVowel && !prevWasVowel) count++;
				prevWasVowel = isVowel;
			}

			return Math.Max(1, count);
		}

		private Dictionary<string, double> CalculateKeywordDensity(string text, int topN)
		{
			var words = Regex.Matches(text.ToLower(), @"\b[\w']+\b")
				.Cast<Match>()
				.Select(m => m.Value)
				.Where(w => w.Length > 3)
				.ToList();

			var totalWords = words.Count;
			if (totalWords == 0) return new Dictionary<string, double>();

			return words
				.GroupBy(w => w)
				.ToDictionary(g => g.Key, g => Math.Round((double)g.Count() / totalWords * 100, 2))
				.OrderByDescending(kv => kv.Value)
				.Take(topN)
				.ToDictionary(kv => kv.Key, kv => kv.Value);
		}

		private string FormatAnalysisResults(Dictionary<string, object> results)
		{
			var output = new System.Text.StringBuilder();
			output.AppendLine("=== ОСНОВНІ МЕТРИКИ ===");
			output.AppendLine();
			output.AppendLine($"{"Довжина тексту:",-25} {results["Length"],10} символів");
			output.AppendLine($"{"Кількість слів:",-25} {results["WordCount"],10}");
			output.AppendLine($"{"Кількість речень:",-25} {results["SentenceCount"],10}");
			output.AppendLine($"{"Кількість абзаців:",-25} {results["ParagraphCount"],10}");
			output.AppendLine($"{"Середня довжина слова:",-25} {Math.Round((double)results["AvgWordLength"], 2),10} символів");
			output.AppendLine($"{"Середня довжина речення:",-25} {Math.Round((double)results["AvgSentenceLength"], 2),10} слів");
			output.AppendLine($"{"Унікальні слова:",-25} {results["UniqueWords"],10}");
			output.AppendLine($"{"Лексичне різноманіття:",-25} {Math.Round((double)results["LexicalDiversity"] * 100, 2),10}%");
			output.AppendLine($"{"Оцінка читабельності:",-25} {Math.Round((double)results["ReadabilityScore"], 2),10} (100 - дуже легко)");
			output.AppendLine();

			output.AppendLine("=== ЩІЛЬНІСТЬ КЛЮЧОВИХ СЛІВ (ТОП-5) ===");
			output.AppendLine();

			foreach (var kv in (Dictionary<string, double>)results["KeywordDensity"])
			{
				output.AppendLine($"{"• " + kv.Key + ":",-20} {kv.Value,6}%");
			}
			output.AppendLine();

			output.AppendLine("=== ІНТЕРПРЕТАЦІЯ ===");
			output.AppendLine();

			int wordCount = (int)results["WordCount"];
			double lexicalDiversity = (double)results["LexicalDiversity"];
			double readability = (double)results["ReadabilityScore"];

			if (wordCount < 50)
			{
				output.AppendLine("Текст занадто короткий для глибокого аналізу.");
			}
			else
			{
				if (lexicalDiversity > 0.7) output.AppendLine("• Високе лексичне різноманіття");
				else if (lexicalDiversity > 0.5) output.AppendLine("• Середнє лексичне різноманіття");
				else output.AppendLine("• Низьке лексичне різноманіття");
				output.AppendLine();

				if (readability > 80) output.AppendLine("• Дуже легкий для читання");
				else if (readability > 60) output.AppendLine("• Досить легкий текст");
				else if (readability > 40) output.AppendLine("• Складний текст");
				else output.AppendLine("• Дуже складний текст");
			}

			return output.ToString();
		}
		private void Form1_Load(object sender, EventArgs e)
		{

		}
	}
}
