using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Newtonsoft.Json;

namespace AskGemini_wpf
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly string _apiKey = "AIzaSyD7iuKtH1Da29D8HUu9gKW4FugYXeJ3gpE";
        private static readonly HttpClient _httpClient = new HttpClient();
        private readonly string _model = "gemini-2.5-flash-lite";

        public MainWindow()
        {
            InitializeComponent();
        }


        public async Task<string> SendRequestAsync(string prompt, string systemInstruction = null)
        {
            try
            {
                string combinedText = string.IsNullOrEmpty(systemInstruction)
            ? prompt
            : $"System Instructions: {systemInstruction}\n\nUser Prompt: {prompt}";

                var payload = new
                {
                    contents = new[]
                    {
                new { parts = new[] { new { text = combinedText } } }
            }
                };
                string url = $"https://generativelanguage.googleapis.com/v1/models/{_model}:generateContent?key={_apiKey}";

                string json = JsonConvert.SerializeObject(payload);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                HttpResponseMessage response = _httpClient.PostAsync(url, content).Result;

                if (!response.IsSuccessStatusCode)
                {
                    var errorBody = await response.Content.ReadAsStringAsync();
                    System.Diagnostics.Debug.WriteLine($"API Error: {errorBody}");
                    return string.Empty;
                }

                string respJson = await response.Content.ReadAsStringAsync();
                var respObj = JsonConvert.DeserializeObject<GeminiResponse>(respJson);

                return respObj?.Candidates?[0]?.Content?.Parts?[0]?.Text ?? string.Empty;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Exception: {ex.Message}");
                return string.Empty;
            }
        }

        private void RoleIdea_Click(object sender, MouseButtonEventArgs e)
        {
            if (sender is System.Windows.Controls.Border border &&
              border.Child is System.Windows.Controls.TextBlock text)
            {
                RoleTextBox.Text = text.Text;
            }
        }
        private async void SendButton_Click(object sender, RoutedEventArgs e)
        {
            string role = RoleTextBox.Text.Trim();
            string prompt = PromptTextBox.Text.Trim();

            if (string.IsNullOrWhiteSpace(role))
            {
                MessageBox.Show("Please enter a role.");
                return;
            }

            if (string.IsNullOrWhiteSpace(prompt))
            {
                MessageBox.Show("Please enter a prompt.");
                return;
            }

            ResponseTextBlock.Text = "Sending request...";

            string response = await SendRequestAsync(prompt, role);

            ResponseTextBlock.Text = response;
        }
    }
}
