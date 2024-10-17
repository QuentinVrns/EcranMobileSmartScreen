using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using DesigneFinal.View;
using Microsoft.Maui.Controls;
using Newtonsoft.Json;


namespace EcranMobileSmartScreen
{
    public partial class MainPage : ContentPage
    {
        private string selectedSalle;

        public MainPage()
        {
            InitializeComponent();
            LoadSalles();
        }

        public object previousContent; // Variable pour stocker la vue précédente

        // Classe pour représenter les salles
        public class Salle
        {
            public int Id { get; set; }
            public string NomSalle { get; set; }
            public int EtageId { get; set; }
        }

        // Fonction pour charger les salles depuis l'API
        private async void LoadSalles()
        {
            string url = "https://quentinvrns.alwaysdata.net/getAllClasse";
            string token = "Bearer eyJ0eXAiOiJKV1QiLCJhbGciOiJIUzI1NiJ9.eyJpYXQiOjE3Mjc2MzA0ODMsImV4cCI6MTAxNzI3NjMwNDgzLCJkYXRhIjp7ImlkIjoxLCJ1c2VybmFtZSI6IlF1ZW50aW4ifX0.k7m0hTQ4-6H7mEI9IPcwvtGdjxqk7q_vip-dRCjwavk";

            using (HttpClient client = new HttpClient())
            {
                try
                {
                    client.DefaultRequestHeaders.Add("Authorization", token);

                    HttpResponseMessage response = await client.GetAsync(url);

                    if (!response.IsSuccessStatusCode)
                    {
                        await DisplayAlert("Erreur", $"Erreur lors de la récupération des salles : {response.StatusCode} - {response.ReasonPhrase}", "OK");
                        return;
                    }

                    string responseBody = await response.Content.ReadAsStringAsync();

                    if (string.IsNullOrEmpty(responseBody))
                    {
                        await DisplayAlert("Information", "La réponse de l'API est vide.", "OK");
                        return;
                    }

                    List<Salle> salles = JsonConvert.DeserializeObject<List<Salle>>(responseBody);

                    if (salles == null || salles.Count == 0)
                    {
                        await DisplayAlert("Information", "Aucune salle disponible.", "OK");
                        return;
                    }

                    // Remplir le Picker avec les salles
                    sallePicker.Items.Clear();
                    foreach (var salle in salles)
                    {
                        sallePicker.Items.Add(salle.NomSalle);
                    }
                }
                catch (HttpRequestException httpEx)
                {
                    await DisplayAlert("Erreur Réseau", $"Problème de connexion : {httpEx.Message}", "OK");
                }
                catch (JsonException jsonEx)
                {
                    await DisplayAlert("Erreur JSON", $"Erreur de traitement des données JSON : {jsonEx.Message}", "OK");
                }
                catch (Exception ex)
                {
                    await DisplayAlert("Erreur", $"Erreur inconnue : {ex.Message}", "OK");
                }
            }
        }

        // Gestion de la sélection dans le Picker
        private void SallePicker_SelectedIndexChanged(object sender, EventArgs e)
        {
            selectedSalle = sallePicker.SelectedItem as string;
        }

        // Gestion du clic sur le bouton de validation
        private async void ValidateButton_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(selectedSalle))
            {
                await DisplayAlert("Erreur", "Veuillez sélectionner une salle.", "OK");
                return;
            }

            // Naviguer vers SecondView avec la salle sélectionnée
            await Navigation.PushAsync(new SecondView(selectedSalle));
        }


        private void QuitButton_Click(object sender, EventArgs e)
        {
            // Pour quitter l'application sur les plateformes appropriées
            Application.Current.Quit();
        }
    }
}
