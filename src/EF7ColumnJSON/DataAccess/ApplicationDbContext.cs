using Azure;
using Azure.AI.OpenAI;
using EF7ColumnJSON.Entities;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace ConsoleApp1.DataAccess
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<Post> Posts { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            //User
            modelBuilder.Entity<User>().HasKey(t => t.Id);

            modelBuilder.Entity<User>().Property(t => t.Username)
                .HasMaxLength(100)
                .IsRequired();

            //SE CONFIGURA LA RELACIÓN DE USER CON ADDRESS Y SE CONFIGURA ADDRESS
            //COMO COLUMNA JSON
            modelBuilder.Entity<User>()
                //LA ENTIDAD ADDRESS NO DISPONE DE UN ID, POR LO QUE NO SE PUEDE
                //RELACIONAR AUTOMÁTICAMENTE EN EL MODELO USER CON ADDRESS USANDO LA PROPIEDAD
                //AddressId, ASÍ QUE ES NECESARIO ASIGNAR ESTA RELACIÓN EN EL CONTEXTO CON .OwnsOne
                .OwnsOne(b => b.Address, ownedNavigationBuilder =>
                {
                    ownedNavigationBuilder.ToJson();
                });
            #region Post/Comment
            //Post
            modelBuilder.Entity<Post>().HasKey(t => t.Id);

            modelBuilder.Entity<Post>().Property(t => t.Title)
                .HasMaxLength(200)
                .IsRequired();

            modelBuilder.Entity<Post>().Property(t => t.Body)
                .HasMaxLength(4000)
                .IsRequired();

            //Comment
            modelBuilder.Entity<Comment>().HasKey(t => t.Id);

            modelBuilder.Entity<Comment>().Property(t => t.Name)
                .HasMaxLength(100)
                .IsRequired();

            modelBuilder.Entity<Comment>().Property(t => t.Body)
                .HasMaxLength(1000)
                .IsRequired();
            #endregion
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
        }

        public static async Task InitializeAsync(ApplicationDbContext context)
        {
            await context.Database.EnsureDeletedAsync();

            await context.Database.MigrateAsync();

            // seed data
            var jsonPath = @"data.json";
            var json = File.ReadAllText(jsonPath);
            var posts = Newtonsoft.Json.JsonConvert.DeserializeObject<List<Post>>(json);


            //EJEMPLO DE QUE TOJSON() ES COMPLETAMENTE USABLE CON LINQ
            posts[0].User.Address = new Address("Street 1", "Sevilla", "Sevilla", "Spain", "111");
            posts[1].User.Address = new Address("Street 2", "Madrid", "Madrid", "Spain", "222");
            posts[2].User.Address = new Address("Street 3", "Barcelona", "Barcelona", "Spain", "333");

            //AÑADIMOS LOS POSTS A LA BDD MEDIANTE EF
            context.AddRange(posts);
            context.SaveChanges();

            var POST1 = context.Posts.FirstOrDefault();

            //LLAMADA A LA API DE OPENAI PARA OBTENER NUEVOS REGISTROS DUMMY
            var jsonPrompt = Newtonsoft.Json.JsonConvert.SerializeObject(context.Posts.FirstOrDefault(), Formatting.Indented,
new JsonSerializerSettings
{
    ReferenceLoopHandling = ReferenceLoopHandling.Ignore
});
            int regsNum = 5;

            //jsonPath = @"json1.json";
            //json = File.ReadAllText(jsonPath);

            var gptPosts = Newtonsoft.Json.JsonConvert.DeserializeObject<List<Post>>(await GptRequest($"based on this json: [{jsonPrompt}] create {regsNum} more dummy data"));
            //var posts1 = Newtonsoft.Json.JsonConvert.DeserializeObject<List<Post>>(json);

            //AÑADIMOS LOS NUEVOS REGISTROS
            if (gptPosts != null) { context.AddRange(gptPosts); }
            context.SaveChanges();

            //BUSCAMOS Y ELIMINAMOS REGISTROS SEGUN SU DIRECCIÓN
            IEnumerable<Post> filteredPosts = context.Posts.Where(i => i.User.Address.City.Equals("Sevilla")).ToList();
            IEnumerable<User> filteredUsers = filteredPosts.Select(i => i.User).ToList();

            context.Users.RemoveRange(filteredUsers);
            context.RemoveRange(filteredPosts);

            context.SaveChanges();

            //COMPROBAMOS QUE ESOS REGISTROS YA NO EXISTEN
            var remainingPosts = context.Posts;
            var remainigUsers = remainingPosts.Select(i => i.User).ToList();

            foreach (var item in remainingPosts)
            {
                Console.WriteLine(Newtonsoft.Json.JsonConvert.SerializeObject(item, Formatting.Indented,
new JsonSerializerSettings
{
    ReferenceLoopHandling = ReferenceLoopHandling.Ignore
}));
            }

            foreach (var item in remainigUsers)
            {
                Console.WriteLine(Newtonsoft.Json.JsonConvert.SerializeObject(item, Formatting.Indented,
new JsonSerializerSettings
{
    ReferenceLoopHandling = ReferenceLoopHandling.Ignore
}));
            }





            async Task<string> GptRequest(string prompt)
            {
                OpenAIClient client = new OpenAIClient(
                                      new Uri("https://pruebaopenaiatmirajavi.openai.azure.com/"),
                                      new AzureKeyCredential("f536dfe16ca74b689336762fd7a5fed4"));

                Response<ChatCompletions> responseWithoutStream = await client.GetChatCompletionsAsync("gtp35prueba",
                    new ChatCompletionsOptions()
                    {
                        Messages =
                        {
                        new ChatMessage(ChatRole.System,
                                        prompt),
                        },
                        Temperature = (float)0.7,
                        MaxTokens = 2000,
                        NucleusSamplingFactor = (float)0.95,
                        FrequencyPenalty = 0,
                        PresencePenalty = 0,
                    });

                ChatCompletions completions = responseWithoutStream.Value;

                return completions.Choices[0].Message.Content;
            }
        }

    }
}
