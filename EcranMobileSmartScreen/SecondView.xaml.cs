using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Dispatching;
using uPLibrary.Networking.M2Mqtt;
using uPLibrary.Networking.M2Mqtt.Messages;

namespace DesigneFinal.View
{
    public partial class SecondView : ContentPage
    {
        private MqttClient client;
        private string salleName;
        private bool isImageLoopRunning = false;
        private List<string> currentImages;
        private List<string> quotes;
        private int currentQuoteIndex = 0;

        public SecondView(string salleName)
        {
            InitializeComponent();
            this.salleName = salleName;
            currentImages = new List<string>();
            quotes = new List<string>();

            LoadImages(salleName);
            LoadQuotes();
            InitializeDateTime();
            InitializeQuoteTimer();
            InitializeMqttClient();
            InitializeAlertMonitoring();
        }

        private async void InitializeMqttClient()
        {
            if (client != null && client.IsConnected)
            {
                return;
            }

            string brokerAddress = "TQN"; // Remplacez par l'adresse IP correcte
            string raspberryPiIp = await GetRaspberryPiIp(); // Récupérer l'IP du Raspberry Pi

            if (string.IsNullOrEmpty(raspberryPiIp))
            {
                await DisplayAlert("Erreur", "Impossible de récupérer l'adresse IP du Raspberry Pi.", "OK");
                return;
            }

            try
            {
                await Task.Run(() =>
                {
                    Console.WriteLine($"Tentative de connexion au broker à l'adresse : {brokerAddress}");
                    client = new MqttClient(brokerAddress);

                    client.MqttMsgPublishReceived += Client_MqttMsgPublishReceived;

                    string clientId = Guid.NewGuid().ToString();
                    client.Connect(clientId, "Taha", "Taha");

                    client.Subscribe(new string[] {
                "Batiment_3/1er/KM_102/Afficheur_n_1/Capteur_temperature_et_humidité",
                "Batiment_3/1er/KM_102/Afficheur_n_1/Capteur_de_CO2",
                "Batiment_3/1er/KM_102/Afficheur_n_1/Capteur_de_son"
            },
                    new byte[] { MqttMsgBase.QOS_LEVEL_AT_LEAST_ONCE });
                });
            }
            catch (Exception ex)
            {
                await DisplayAlert("Erreur de connexion", $"Connexion MQTT impossible : {ex.Message}", "OK");
            }
        }

        private async Task<string> GetRaspberryPiIp()
        {
            string ipCheckUrl = "http://192.168.7.148:5000/ip"; // Endpoint à créer sur le Raspberry Pi

            try
            {
                using (HttpClient client = new HttpClient())
                {
                    string ip = await client.GetStringAsync(ipCheckUrl);
                    return ip; // Retourne l'IP récupérée
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erreur lors de la récupération de l'IP : {ex.Message}");
                return null; // Retourne null en cas d'erreur
            }
        }


        private void Client_MqttMsgPublishReceived(object sender, MqttMsgPublishEventArgs e)
        {
            string message = Encoding.UTF8.GetString(e.Message);
            string topic = e.Topic;

            MainThread.BeginInvokeOnMainThread(() =>
            {
                if (topic.Contains("Capteur_temperature_et_humidité"))
                {
                    string temperature = ExtractValue(message, "Temp", "C");
                    string humidity = ExtractValue(message, "Humidity", "%");

                    TBtemp.Text = $"Température {temperature} °C";
                    TBhum.Text = $"Humidité {humidity}%";
                }
                else if (topic.Contains("Capteur_de_CO2"))
                {
                    string pm25 = ExtractValue(message, "PM2.5", "microg/m³");
                    string pm10 = ExtractValue(message, "PM10", "microg/m³");

                    TBPM2.Text = $"PM2.5 {pm25} µg/m³";
                    TBPM10.Text = $"PM10 {pm10} µg/m³";
                }
                else if (topic.Contains("Capteur_de_son"))
                {
                    TBson.Text = $"Son : {message} dB";
                }
            });
        }

        private string ExtractValue(string message, string key, string delimiter)
        {
            try
            {
                int startIndex = message.IndexOf(key) + key.Length;
                int endIndex = message.IndexOf(delimiter, startIndex);

                if (startIndex >= 0 && endIndex > startIndex)
                {
                    return message.Substring(startIndex, endIndex - startIndex).Trim();
                }

                return "N/A";
            }
            catch
            {
                return "N/A";
            }
        }

        private async void LoadQuotes()
        {
            string quoteUrl = "https://quentinvrns.fr/Document/citation.txt";

            try
            {
                using (HttpClient client = new HttpClient())
                {
                    string content = await client.GetStringAsync(quoteUrl);
                    var matches = Regex.Matches(content, @"\d+\.\s""([^""]+)""\s–\s(.+)");

                    foreach (Match match in matches)
                    {
                        if (match.Groups.Count >= 3)
                        {
                            string citation = $"{match.Groups[1].Value} – {match.Groups[2].Value}";
                            quotes.Add(citation);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Erreur", $"Erreur lors de la récupération des citations : {ex.Message}", "OK");
            }

            if (quotes.Count > 0)
            {
                TBinfo.Text = quotes[currentQuoteIndex];
            }
        }

        private void InitializeQuoteTimer()
        {
            Dispatcher.StartTimer(TimeSpan.FromHours(12), () =>
            {
                if (quotes.Count > 0)
                {
                    currentQuoteIndex = (currentQuoteIndex + 1) % quotes.Count;
                    TBinfo.Text = quotes[currentQuoteIndex];
                }
                return true;
            });
        }

        private void InitializeDateTime()
        {
            Dispatcher.StartTimer(TimeSpan.FromSeconds(1), () =>
            {
                TBdate.Text = DateTime.Now.ToString("dddd dd MMMM yyyy");
                TBheure.Text = DateTime.Now.ToString("HH:mm");
                return true;
            });
        }

        private async void LoadImages(string salleName)
        {
            try
            {
                string baseImageUrl = "https://quentinvrns.fr/Document/";
                string salleUrl = $"{baseImageUrl}{salleName}/";
                string listUrl = $"{salleUrl}image.json";

                await FetchAndDisplayImages(salleUrl);
            }
            catch (Exception ex)
            {
                await DisplayAlert("Erreur", $"Erreur lors du chargement des images : {ex.Message}", "OK");
            }
        }

        private async Task FetchAndDisplayImages(string salleUrl)
{
    List<string> availableImages = await GetAvailableImages();

    Console.WriteLine($"Images disponibles : {string.Join(", ", availableImages)}"); // Journaliser les images récupérées

    if (availableImages == null || availableImages.Count == 0)
    {
        await DisplayAlert("Erreur", "Aucune image disponible pour la salle sélectionnée.", "OK");
        return;
    }

    currentImages = availableImages;
    isImageLoopRunning = true;

    await DisplayImagesLoop(salleUrl);
}

        private async Task<List<string>> GetAvailableImages()
        {
            List<string> availableImages = new List<string>();

            try
            {
                using (HttpClient client = new HttpClient())
                {
                    HttpResponseMessage response = await client.GetAsync($"https://quentinvrns.fr/Document/{salleName}/image.json");
                    if (response.IsSuccessStatusCode)
                    {
                        string responseBody = await response.Content.ReadAsStringAsync();
                        availableImages = JsonConvert.DeserializeObject<List<string>>(responseBody);
                    }
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Erreur", $"Erreur lors de la récupération des images : {ex.Message}", "OK");
            }

            return availableImages;
        }

        private async Task DisplayImagesLoop(string salleUrl)
        {
            while (isImageLoopRunning)
            {
                await CheckAlertsAsync(); // Vérifier les alertes avant d'afficher les images

                foreach (var image in currentImages)
                {
                    string imageUrl = $"{salleUrl}{image}";
                    imageControl.Source = ImageSource.FromUri(new Uri(imageUrl));

                    await Task.Delay(3000); // Attendre 3 secondes entre chaque image
                }
            }
        }

        private void InitializeAlertMonitoring()
        {
            Dispatcher.StartTimer(TimeSpan.FromSeconds(2), () =>
            {
                _ = CheckAlertsAsync(); // Appeler la méthode CheckAlertsAsync
                return true; // Continuer le timer
            });
        }

        private async Task CheckAlertsAsync()
        {
            string alertUrl = "https://quentinvrns.fr/Document/alerte.txt";

            try
            {
                using (HttpClient client = new HttpClient())
                {
                    string alertContent = await client.GetStringAsync(alertUrl);

                    if (alertContent.Contains("ALERTE INCENDIE"))
                    {
                        DisplayAlert("ALERTE INCENDIE");
                    }
                    else if (alertContent.Contains("ALERTE INTRUSION"))
                    {
                        DisplayAlert("ALERTE INTRUSION");
                    }
                    else if (alertContent.Contains("ALERTE EVACUATION AUTRE DANGER"))
                    {
                        DisplayAlert("ALERTE EVACUATION AUTRE DANGER");
                    }
                    else
                    {
                        ClearAlert();
                    }
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Erreur", $"Erreur lors de la récupération des alertes : {ex.Message}", "OK");
            }
        }

        private void DisplayAlert(string alertType)
        {
            isImageLoopRunning = false;
            fullScreenAlertImage.IsVisible = true;

            if (alertType.Contains("INCENDIE"))
            {
                fullScreenAlertImage.Source = "incendie.png";
            }
            else if (alertType.Contains("INTRUSION"))
            {
                fullScreenAlertImage.Source = "intrusion.png";
            }
            else if (alertType.Contains("DANGER"))
            {
                fullScreenAlertImage.Source = "evacuation.png";
            }
        }

        private void ClearAlert()
        {
            isImageLoopRunning = true;
            fullScreenAlertImage.IsVisible = false;
        }
    }
}
