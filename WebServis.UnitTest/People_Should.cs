using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;
using WebServis.ApiControllers;
using WebServis.DAL.Entities;
using Xunit;

namespace WebServis.UnitTest
{
    public class People_Should
    {
        DbContextOptions<AdventureWorks2014Context> _dbContextOptions;

        public People_Should()
        {
            // Kreiranje opcija za InMemory testnu bazu podataka
            _dbContextOptions = new DbContextOptionsBuilder<AdventureWorks2014Context>()
                            .UseInMemoryDatabase(databaseName: "Test_database")
                            .Options;
        }
        
        [Fact]
        public async void PostPerson()
        {
            // Koristenje InMemory baze podataka (context)
            using (var context = new AdventureWorks2014Context(_dbContextOptions))
            {
                var peopleAPI = new PeopleController(context);
                for (int i = 0; i < 10; ++i)
                {
                    var tmpperson = new Person();
                    tmpperson.PersonType = $"Tip { i + 1 }";
                    tmpperson.FirstName = "Ime" + i;
                    tmpperson.LastName = "Prezime" + i;
                    var result = await peopleAPI.PostPerson(tmpperson);
                    var badRequest = result as BadRequestObjectResult;

                    Assert.Null(badRequest);    // Ako API ne vraca BadRequest, to znaci da je poziv uspjesan
                }
            }
        }
        
        [Fact]
        public async void GetPerson()
        {
            using (var context = new AdventureWorks2014Context(_dbContextOptions))
            {
                var peopleAPI = new PeopleController(context);

                    var tmpperson = new Person();
                    tmpperson.PersonType = $"Tip";
                    tmpperson.FirstName = "Ime";
                    tmpperson.LastName = "Prezime";
                    peopleAPI.PostPerson(tmpperson).Wait();
            }

            using (var context = new AdventureWorks2014Context(_dbContextOptions))
            {
                var peopleAPI = new PeopleController(context);
                var result = await peopleAPI.GetPerson(1);
                var okResult = result as OkObjectResult;
                
                Assert.NotNull(okResult);
                Assert.Equal(200, okResult.StatusCode);

                var person = okResult.Value as Person;
                Assert.NotNull(person);
                Assert.Equal("Prezime", person.LastName);
            }
        }
    }
}