using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WeatherApp.Helper;
using WeatherApp.Models;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace WeatherApp.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class CurrentWeatherPage : ContentPage
    {
        public CurrentWeatherPage()
        {
            InitializeComponent();
            GetWeatherInfo();
        }

        private string Location = "Lanaken";

        private async void GetWeatherInfo()
        {
            //Location wordt uit de string Location gehaald
            var url = $"http://api.openweathermap.org/data/2.5/weather?q=" + Location + "&APPID=70c9d17ec1211e30b75ac292a72d685e&units=metric";

            var result = await ApiCaller.Get(url);

            if (result.Succesful)
            {
                try
                {
                    //converteer de JSON en steek het in result
                    var weatherInfo = JsonConvert.DeserializeObject<WeatherInfo>(result.Response);

                    //steek de weerinfo in de corresponderende vakken
                    descriptionTxt.Text = weatherInfo.weather[0].description.ToUpper();
                    iconImg.Source = $"w{weatherInfo.weather[0].icon}";
                    cityTxt.Text = weatherInfo.name.ToUpper();
                    temperatureTxt.Text = weatherInfo.main.temp.ToString("0");
                    humidityTxt.Text = $"{weatherInfo.main.humidity}%";
                    pressureTxt.Text = $"{weatherInfo.main.pressure} hpa";
                    windTxt.Text = $"{weatherInfo.wind.speed} m/s";
                    cloudinessTxt.Text = $"{weatherInfo.clouds.all}%";

                    //converie van het UNIX UTC tijdsformaat naar algemeen datumformaat
                    var dt = new DateTime().ToUniversalTime().AddSeconds(weatherInfo.dt);
                    dateTxt.Text = dt.ToString("dddd, MMM dd").ToUpper();

                }
                catch (Exception ex)
                {
                    //errormessage bij exception en corresponderende message
                    await DisplayAlert("Weather Info", ex.Message, "OK");

                }

            }
            else
            {
                //indien er geen resultaat opgehaald kan worden wordt deze errormessage weergegeven
                await DisplayAlert("Weather Info", "No weatherinformation found", "OK");
            }
        }
    }
}