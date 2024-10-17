using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;

namespace DesigneFinal.View
{
    public partial class Meteo : ContentPage
    {
        public Meteo()
        {
            InitializeComponent();
            GetMeteo("Annecy");
        }

        public async Task<string> GetMeteo(string city)
        {
            HttpClient client = new HttpClient();
            try
            {
                HttpResponseMessage response = await client.GetAsync($"https://www.prevision-meteo.ch/services/json/{city}");
                if (response.IsSuccessStatusCode)
                {
                    string result = await response.Content.ReadAsStringAsync();
                    Root myDeserializedClass = JsonConvert.DeserializeObject<Root>(result);

                    if (result.Contains("error"))
                    {
                        await DisplayAlert("Erreur", "Ville indisponible", "OK");
                        return "";
                    }

                    // Info for today
                    FcstDay0 fcstDay0 = myDeserializedClass.fcst_day_0;
                    CurrentCondition currentCondition = myDeserializedClass.current_condition;

                    TB_temperature.Text = currentCondition.tmp.ToString() + "°C";
                    TB_condition.Text = currentCondition.condition;
                    TB_Aujourdhui.Text = fcstDay0.day_long;
                    TB_Humidité.Text = currentCondition.humidity.ToString() + "% d'humidité";
                    TB_bas.Text = "Min : " + fcstDay0.tmin.ToString() + "°C";
                    TB_haut.Text = "Max : " + fcstDay0.tmax.ToString() + "°C";

                    // Load the weather icon for today
                    jour0.Source = ImageSource.FromUri(new Uri(fcstDay0.icon_big));

                    // Info for tomorrow
                    FcstDay1 fcstDay1 = myDeserializedClass.fcst_day_1;
                    TB_Demain.Text = fcstDay1.day_long;
                    TB_basD.Text = "Min : " + fcstDay1.tmin.ToString() + "°C";
                    TB_hautD.Text = "Max : " + fcstDay1.tmax.ToString() + "°C";
                    jour1.Source = ImageSource.FromUri(new Uri(fcstDay1.icon_big));

                    // Info for day after tomorrow
                    FcstDay2 fcstDay2 = myDeserializedClass.fcst_day_2;
                    TB_ApresDemain.Text = fcstDay2.day_long;
                    TB_basAD.Text = "Min : " + fcstDay2.tmin.ToString() + "°C";
                    TB_hautAD.Text = "Max : " + fcstDay2.tmax.ToString() + "°C";
                    jour2.Source = ImageSource.FromUri(new Uri(fcstDay2.icon_big));

                    // Info for three days later
                    FcstDay3 fcstDay3 = myDeserializedClass.fcst_day_3;
                    TB_Dans3Jours.Text = fcstDay3.day_long;
                    TB_bas3J.Text = "Min : " + fcstDay3.tmin.ToString() + "°C";
                    TB_haut3J.Text = "Max : " + fcstDay3.tmax.ToString() + "°C";
                    jour3.Source = ImageSource.FromUri(new Uri(fcstDay3.icon_big));

                    return "";
                }
                else
                {
                    return "Error";
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Erreur", $"Erreur : {ex.Message}", "OK");
                return ex.Message;
            }
        }

        // JSON classes for deserialization of weather data
        public class Root
        {
            public CityInfo city_info { get; set; }
            public CurrentCondition current_condition { get; set; }
            public FcstDay0 fcst_day_0 { get; set; }
            public FcstDay1 fcst_day_1 { get; set; }
            public FcstDay2 fcst_day_2 { get; set; }
            public FcstDay3 fcst_day_3 { get; set; }
        }

        public class CityInfo
        {
            public string name { get; set; }
            public string country { get; set; }
        }

        public class CurrentCondition
        {
            public int tmp { get; set; }
            public int humidity { get; set; }
            public string condition { get; set; }
        }

        public class FcstDay0
        {
            public string day_long { get; set; }
            public int tmin { get; set; }
            public int tmax { get; set; }
            public string icon_big { get; set; }
        }

        public class FcstDay1
        {
            public string day_long { get; set; }
            public int tmin { get; set; }
            public int tmax { get; set; }
            public string icon_big { get; set; }
        }

        public class FcstDay2
        {
            public string day_long { get; set; }
            public int tmin { get; set; }
            public int tmax { get; set; }
            public string icon_big { get; set; }
        }

        public class FcstDay3
        {
            public string day_long { get; set; }
            public int tmin { get; set; }
            public int tmax { get; set; }
            public string icon_big { get; set; }
        }
    }
}
