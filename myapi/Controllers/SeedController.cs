﻿using CsvHelper;
using CsvHelper.Configuration;
using dataModel;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using myapi.Data;
using System;
using System.Globalization;

namespace myapi.Controllers

{
    [Route("api/[controller]")]
    [ApiController]
    public class SeedController(WorldDatabaseContext context, IHostEnvironment environment) : ControllerBase
    {
        private readonly string _pathName = Path.Combine(environment.ContentRootPath, "Data/worldcities.csv");
        [HttpPost("Countries")]
        public async Task<IActionResult> ImportCountriesAsync()
        {
            // create a lookup dictionary containing all the countries already existing 
            // into the Database (it will be empty on first run).
            Dictionary<string, Country> countriesByName = context.Countries
                .AsNoTracking().ToDictionary(x => x.Name, StringComparer.OrdinalIgnoreCase);

            CsvConfiguration config = new(CultureInfo.InvariantCulture)
            {
                HasHeaderRecord = true,
                HeaderValidated = null
            };

            using StreamReader reader = new(_pathName);
            using CsvReader csv = new(reader, config);

            List<WorldCitiesCSV> records = csv.GetRecords<WorldCitiesCSV>().ToList();
            foreach (WorldCitiesCSV record in records)
            {
                if (countriesByName.ContainsKey(record.country))
                {
                    continue;
                }

                Country country = new()
                {
                    Name = record.country,
                    Iso2 = record.iso2,
                    Iso3 = record.iso3
                };
                await context.Countries.AddAsync(country);
                countriesByName.Add(record.country, country);
            }

            await context.SaveChangesAsync();

            return new JsonResult(countriesByName.Count);
        }

        [HttpPost("Cities")]
        public async Task<IActionResult> ImportCitiesAsync()
        {
            Dictionary<string, Country> countries = await context.Countries//.AsNoTracking()
            .ToDictionaryAsync(c => c.Name);

            CsvConfiguration config = new(CultureInfo.InvariantCulture)
            {
                HasHeaderRecord = true,
                HeaderValidated = null
            };
            int cityCount = 0;
            using (StreamReader reader = new(_pathName))
            using (CsvReader csv = new(reader, config))
            {
                IEnumerable<WorldCitiesCSV>? records = csv.GetRecords<WorldCitiesCSV>();
                foreach (WorldCitiesCSV record in records)
                {
                    if (!countries.TryGetValue(record.country, out Country? value))
                    {
                        Console.WriteLine($"Not found country for {record.city}");
                        return NotFound(record);
                    }

                    if (!record.population.HasValue || string.IsNullOrEmpty(record.city_ascii))
                    {
                        Console.WriteLine($"Skipping {record.city}");
                        continue;
                    }
                    City city = new()
                    {
                        Name = record.city,
                        Lat = record.lat,
                        Lng = record.lng,
                        Population = (int)record.population.Value,
                        CountryId = value.Id
                    };
                    context.Cities.Add(city);
                    cityCount++;
                }
                await context.SaveChangesAsync();
            }
            return new JsonResult(cityCount);
        }
    }
}
