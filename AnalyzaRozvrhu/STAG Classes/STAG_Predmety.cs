﻿// Generated by Xamasoft JSON Class Generator
// http://www.xamasoft.com/json-class-generator

using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace AnalyzaRozvrhu.STAG_Classes
{

    public class Predmet
    {

        [JsonProperty("katedra")]
        public string Katedra { get; set; }

        [JsonProperty("zkratka")]
        public string Zkratka { get; set; }

       

        [JsonProperty("kreditu")]
        public int Kreditu { get; set; }

        

        [JsonProperty("jednotekPrednasek")]
        public int JednotekPrednasek { get; set; }

        [JsonProperty("jednotkaPrednasky")]
        public string JednotkaPrednasky { get; set; }

        [JsonProperty("jednotekCviceni")]
        public int JednotekCviceni { get; set; }

        [JsonProperty("jednotkaCviceni")]
        public string JednotkaCviceni { get; set; }

        [JsonProperty("jednotekSeminare")]
        public int JednotekSeminare { get; set; }

        [JsonProperty("jednotkaSeminare")]
        public string JednotkaSeminare { get; set; }
             


    }

    public class PredmetResponse
    {

        [JsonProperty("predmetInfo")]
        public Predmet[] PredmetInfo { get; set; }
    }

}