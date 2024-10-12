﻿using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace myapi.DTO
{
    public class CityDTO
    {

        public int Id { get; set; }


        public string Name { get; set; } = null!;


        public double Lng { get; set; }


        public double Lat { get; set; }


        public int Population { get; set; }
        public required string CountryName { get; set; }
    }
}
