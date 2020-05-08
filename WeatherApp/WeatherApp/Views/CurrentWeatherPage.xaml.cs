using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using WeatherApp.Helper;
using WeatherApp.Models;
using Xamarin.Essentials;
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

            //vraag locatie op
            GetCoordinates();
            


            
        }

        private string Location { get; set; } = "Paris";
        public double Latitude { get; set; }
        public double Longitude { get; set; }

        
        private async void GetCoordinates()
        {
            try
            {
                // doe een geolocatie request met best mogelijke precisie en steek ze in var location
                var request = new GeolocationRequest(GeolocationAccuracy.Best);
                var location = await Geolocation.GetLocationAsync(request);

                if (location !=null)
                {
                    //steek lat en long in corresponderende public variabelen
                    Latitude = location.Latitude;
                    Longitude = location.Longitude;

                    //Vraag de stad op via GetCity()
                    Location = await GetCity(location);

                    //vraag weerinfo nadat de locatie is opgevraagd
                    GetWeatherInfo();

                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.StackTrace);
            }
        }

        private async Task<string> GetCity(Location location)
        {
            var places = await Geocoding.GetPlacemarksAsync(location);
            //? checkt of places niet null is
            var currentplace = places?.FirstOrDefault();

            // indien huidihge plaats verschillend is van nul geef de locatie en land terug
            if (currentplace != null)
                return $"{currentplace.Locality},{currentplace.CountryName}";
            // ander geef null terug
            return null;
        }

        private async void GetWeatherInfo()
        {
            
            //API request van huidige weersituatie voor var Location
            var url = $"http://api.openweathermap.org/data/2.5/weather?q=" + Location + "&APPID=70c9d17ec1211e30b75ac292a72d685e&units=metric";

            var result = await ApiCaller.Get(url);

            if (result.Successful)
            {
                try
                {
                    //converteer de JSON en steek het in result
                    var weatherInfo = JsonConvert.DeserializeObject<WeatherInfo>(result.Response);

                    //steek de weerinfo in de corresponderende vakken
                    descriptionTxt.Text = weatherInfo.weather[0].description.ToUpper();
                    iconImg.Source = $"w{weatherInfo.weather[0].icon}";
                    cityTxt.Text = weatherInfo.name.ToUpper(); // + " " + weatherInfo.sys.country.ToUpper();
                    temperatureTxt.Text = weatherInfo.main.temp.ToString("0");
                    temperatureFeelTxt.Text = "Feels like " + weatherInfo.main.feels_like.ToString("0") + "°";
                    humidityTxt.Text = $"{weatherInfo.main.humidity}%";
                    pressureTxt.Text = $"{weatherInfo.main.pressure} hpa";
                    windTxt.Text = $"{weatherInfo.wind.speed} m/s";
                    cloudinessTxt.Text = $"{weatherInfo.clouds.all}%";
                    // huidige datum invullen
                    dateTxt.Text = DateTime.Now.ToString("dddd, MMM dd").ToUpper();

                    // roep voorspelling aan
                    GetForecast();
                    //zoek voor passende achtergrond
                    GetBackground();

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

        private async void GetForecast()
        {
            // API request van weersvoorspelling van de komende dagen voor var Location
            var url = $"http://api.openweathermap.org/data/2.5/forecast?q=" + Location + "&APPID=70c9d17ec1211e30b75ac292a72d685e&units=metric";

            var result = await ApiCaller.Get(url);

            if (result.Successful)
            {
                try
                {
                    //converteer JSON van weersvoorspelling
                    var forcastInfo = JsonConvert.DeserializeObject<ForecastInfo>(result.Response);

                    List<List> allList = new List<List>();

                    //Steek de voorspelling in een lijst
                    foreach (var list in forcastInfo.list)
                    {
                        //De openweather forecast API geeft de voorspelling op 3 uurlijke basis, we parsen (ontleden) de data
                        var date = DateTime.Parse(list.dt_txt);

                        //enkel de dagen na de huidige dag worden in de voorspelling opgenomen
                        // we kiezen om de situatie rond de middag weer te geven omdat dit de het meest relevante is
                        if (date > DateTime.Now && date.Hour == 12 && date.Minute == 0 && date.Second == 0)
                            allList.Add(list);
                    }

                    //Steek resultaat in kader 1 (morgen)
                    dayOneTxt.Text = DateTime.Parse(allList[0].dt_txt).ToString("dddd");
                    dateOneTxt.Text = DateTime.Parse(allList[0].dt_txt).ToString("dd MMM");
                    iconOneImg.Source = $"w{allList[0].weather[0].icon}";
                    tempOneTxt.Text = allList[0].main.temp.ToString("0");

                    //steek resultaat in kader 2 (overmorgen)
                    dayTwoTxt.Text = DateTime.Parse(allList[1].dt_txt).ToString("dddd");
                    dateTwoTxt.Text = DateTime.Parse(allList[1].dt_txt).ToString("dd MMM");
                    iconTwoImg.Source = $"w{allList[1].weather[0].icon}";
                    tempTwoTxt.Text = allList[1].main.temp.ToString("0");

                    //steek resultaat in kader 3 (3 dagen vanaf nu)
                    dayThreeTxt.Text = DateTime.Parse(allList[2].dt_txt).ToString("dddd");
                    dateThreeTxt.Text = DateTime.Parse(allList[2].dt_txt).ToString("dd MMM");
                    iconThreeImg.Source = $"w{allList[2].weather[0].icon}";
                    tempThreeTxt.Text = allList[2].main.temp.ToString("0");

                    //steek resultaat in kader 4 (4dagen vanaf nu)
                    dayFourTxt.Text = DateTime.Parse(allList[3].dt_txt).ToString("dddd");
                    dateFourTxt.Text = DateTime.Parse(allList[3].dt_txt).ToString("dd MMM");
                    iconFourImg.Source = $"w{allList[3].weather[0].icon}";
                    tempFourTxt.Text = allList[3].main.temp.ToString("0");

                }
                catch (Exception ex)
                {
                    //indien er een exception voordoet geef errormessage met reden weer
                    await DisplayAlert("Weather Info", ex.Message, "OK");
                }
            }
            else
            {
                //indien de API request onsuccesvol is
                await DisplayAlert("Weather Info", "No forecast information found", "OK");
            }
        }

        private async void GetBackground()
        {
            //pexels api url
            var url = $"https://api.pexels.com/v1/search?query={Location}&per_page=15&page1";

            // roep apicaller aan, geef authentication id van pexels mee
            var result = await ApiCaller.Get(url, "563492ad6f91700001000001b02cf24e64f64ca3b1dfb54b865ff812");

            if(result.Successful)
            {
                //converteer de JSON en steek in bgInfo
                var bgInfo = JsonConvert.DeserializeObject<BackgoundInfo>(result.Response);

                //Indien er foto's beschikbaar zijn kies een willekeurige foto
                if(bgInfo != null && bgInfo.photos.Length > 0)
                bgImg.Source = ImageSource.FromUri(
                    new Uri(bgInfo.photos[new Random().Next(0, bgInfo.photos.Length-1)].src.medium));
                
            }
        }

        private void OnRefreshClicked(object sender, EventArgs e)
        {
            //refresh de locatie en weer gegevens
            GetCoordinates();
        }

        private void Entry_Completed(object sender, EventArgs e)
        {
            //verander de locatie naar de door de gebruiker opgegeven locatie en vraag hiervoor de weerinfo op
            Location = cityTxt.Text;
            GetWeatherInfo();
        }
    }
}