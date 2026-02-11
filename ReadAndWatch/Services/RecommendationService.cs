using System.Net.Http;
using System.Text;
using System.Text.Json;
using ReadAndWatch.Models;

namespace ReadAndWatch.Services
{
    public class RecommendationService
    {
        private readonly HttpClient _httpClient;

        public RecommendationService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<List<Book>> GetBookRecommendationsByTitlesAsync(int userId, List<string> favoriteTitles)
        {
            if (favoriteTitles == null || !favoriteTitles.Any())
                return new List<Book>();

            var payload = new
            {
                titles = favoriteTitles,
                user_id = userId
            };

            var content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");

            try
            {
                var response = await _httpClient.PostAsync("http://127.0.0.1:8000/recommend", content);
                response.EnsureSuccessStatusCode();

                var responseBody = await response.Content.ReadAsStringAsync();
                var books = JsonSerializer.Deserialize<List<Book>>(responseBody);

                return books ?? new List<Book>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Flask kitap öneri API hatası: {ex.Message}");
                return new List<Book>();
            }
        }



        public async Task<List<Movie>> GetMovieRecommendationsByTitlesAsync(int userId, List<string> favoriteTitles)
        {
            if (favoriteTitles == null || !favoriteTitles.Any())
                return new List<Movie>();

            var payload = new
            {
                titles = favoriteTitles,
                user_id = userId
            };

            var content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");

            try
            {
                var response = await _httpClient.PostAsync("http://127.0.0.1:8001/recommend_movies", content);


                response.EnsureSuccessStatusCode();

                var responseBody = await response.Content.ReadAsStringAsync();
                var movies = JsonSerializer.Deserialize<List<Movie>>(responseBody);

                return movies ?? new List<Movie>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Flask film öneri API hatası: {ex.Message}");
                return new List<Movie>();
            }
        }
    }
}
