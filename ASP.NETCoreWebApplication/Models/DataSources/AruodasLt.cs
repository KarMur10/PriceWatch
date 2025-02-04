﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using ASP.NETCoreWebApplication.Interactors;
using ASP.NETCoreWebApplication.Utils;
using OpenQA.Selenium;

namespace ASP.NETCoreWebApplication.Models.DataSources
{
    public enum HousingType
    {
        RentFlat,
        BuyFlat,
        RentHouse,
        BuyHouse
    }

    public enum FHouseState
    {
        Full, //IRENGTAS
        Part, //DALINAI IRENGTAS
        Noteq, //Neirengtas
        Nfinished, //Nebaigtas statyti
        Foundation, //Pamatai
        None //Neatitinka jokiu kitu parametru
    }

    public class RangeDescriptor
    {
        public enum RoomRangeType
        {
            Exact, //exactly X rooms
            RangeOf //Rooms from X to Y, will throw InvalidDescriptorException if Y < X
        }

        private RoomRangeType _rangeType;
        private int min;
        private int max;
        private int exactly;

        public class InvalidDescriptorException : Exception
        {
            public InvalidDescriptorException()
            {
                
            }
        }

        public RangeDescriptor(int min, int max)
        {
            if (min > max)
            {
                throw new InvalidDescriptorException();
            }

            if (min < 1 || max < 1)
            {
                //logiskai niekada nebus maziau nei 1 kambario butas
                throw new InvalidDescriptorException();
            }

            this.min = min;
            this.max = max;
            this._rangeType = RoomRangeType.RangeOf;
        }

        public RangeDescriptor(int exactly)
        {
            if (exactly < 1)
            {
                throw new InvalidDescriptorException();
            }

            this.exactly = exactly;
            this._rangeType = RoomRangeType.Exact;
        }

        public bool MatchDescriptor(int rooms)
        {
            if (_rangeType == RoomRangeType.Exact)
            {
                return this.exactly == rooms;
            }
            else
            {
                return rooms >= this.min && rooms <= this.max;
            }
        }

        public RoomRangeType GetRangeType()
        {
            return this._rangeType;
        }

        public int GetMin()
        {
            return this.min;
        }

        public int GetMax()
        {
            return this.max;
        }

        public int GetExactly()
        {
            return this.exactly;
        }

    }

    public class RoomNumberDescriptor : RangeDescriptor
    {
        public RoomNumberDescriptor(int min, int max) : base(min, max)
        {
        }

        public RoomNumberDescriptor(int exactly) : base(exactly)
        {
        }
    }

    public class AreaDescriptor : RangeDescriptor
    {
        public AreaDescriptor(int min, int max) : base(min, max)
        {
        }

        public AreaDescriptor(int exactly) : base(exactly)
        {
        }
    }
    
    public class AruodasLt
    {
        private readonly HousingType housingType;
        private readonly PriceRange priceRange;
        private readonly RoomNumberDescriptor rooms;
        private readonly AreaDescriptor area; //kvadratiniai metrai
        private readonly string? optionalSearch;
        public AruodasLt(HousingType type, RoomNumberDescriptor rooms, AreaDescriptor area, PriceRange priceRange, string? optionalSearchText)
        {
            //assign
            this.housingType = type;
            this.rooms = rooms;
            this.priceRange = priceRange;
            this.area = area;
            this.optionalSearch = optionalSearchText;
            //search
        }

        private string BuildUrl()
        {
            //?FRoomNumMin=10&FRoomNumMax=20 Kambariu skaicius nuo iki
            //?FAreaOverAllMin=20&FAreaOverAllMax=800 Kvadratiniai metrai
            
            //&FHouseState=full PILNAI IRENGTAS
            //&FHouseState=part DALINAI IRENGTAS
            //&FHouseState=noteq NEIRENGTAS
            //&FHouseState=n_finished NEPASTATYTAS
            //&FHouseState=foundation PAMATAI
            //?FHouseState=none Neatitinka jokiu kitu parametru
            
            //?search_text=lazdynai -> ieskos lazdynuose
            
            string location = "";

            switch (housingType)
            {
                case HousingType.BuyFlat:
                {
                    location = "https://www.aruodas.lt/butai/?";
                    break;
                }
                case HousingType.BuyHouse:
                {
                    location = "https://www.aruodas.lt/namai/?";
                    break;
                }
                case HousingType.RentFlat:
                {
                    location = "https://www.aruodas.lt/butu-nuoma/?";
                    break;
                }
                case HousingType.RentHouse:
                {
                    location = "https://www.aruodas.lt/namu-nuoma/";
                    break;
                }
                default:
                {
                    throw new InvalidLocationException();
                }
            }
            
            RoomNumberDescriptor.RoomRangeType rangeType = this.rooms.GetRangeType();
            if (rangeType == RoomNumberDescriptor.RoomRangeType.Exact)
            {
                //?FRoomNumMin=Exact&FRoomNumMax=Exact
                location += "FRoomNumMin=";
                location += this.rooms.GetExactly().ToString();
                location += "&";
                location += "FRoomNumMax=";
                location += this.rooms.GetExactly().ToString();
                location += "&";
            }
            else
            {
                location += "FRoomNumMin=";
                location += this.rooms.GetMin().ToString();
                location += "&";
                location += "FRoomNumMax=";
                location += this.rooms.GetMax().ToString();
                location += "&";
            }

            AreaDescriptor.RoomRangeType areaRangeType = this.area.GetRangeType();
            if (areaRangeType == AreaDescriptor.RoomRangeType.Exact)
            {
                location += "FAreaOverAllMin=";
                location += this.area.GetExactly().ToString();
                location += "&";
                location += "FAreaOverAllMax=";
                location += this.area.GetExactly().ToString();
                location += "&";
            }
            else
            {
                location += "FAreaOverAllMin=";
                location += this.area.GetMin().ToString();
                location += "&";
                location += "FAreaOverAllMax=";
                location += this.area.GetMax().ToString();
                location += "&";
            }

            if (optionalSearch != null)
            {
                location += "search_text=";
                location += optionalSearch;
            }

            ConsoleWriter.WriteHttpGetScrappers(location);
            //remove last & from URLs
            return location;
        }

        public HousingObject[] Scrap(PriceWatchContext dbc, int depth = 4)
        {
            
            if(depth < 1) throw new ArgumentException("depth cannot be zero or negative");
            //get html
            WebDriver wd = SeleniumScrapper.CreateFirefoxDriver();
            wd.Navigate().GoToUrl(this.BuildUrl());
            
            //hardcoding is bad lol
            //TODO move hardcoded values to database

            Dictionary<string, Tuple<string, HTMLNodeParser.ParseOptions>> rawValues = new Dictionary<string, Tuple<string, HTMLNodeParser.ParseOptions>>
            {
                ["price"] = Tuple.Create("span", new HTMLNodeParser.ParseOptions(HTMLNodeParser.ParserFlags.HtmlElementClassName,"list-item-price")),
                ["area"] = Tuple.Create("td", new HTMLNodeParser.ParseOptions(HTMLNodeParser.ParserFlags.HtmlElementClassName,"list-AreaOverall")),
                ["rooms"] = Tuple.Create("td", new HTMLNodeParser.ParseOptions(HTMLNodeParser.ParserFlags.HtmlElementClassName, "list-RoomNum")),
                ["floors"] = Tuple.Create("td", new HTMLNodeParser.ParseOptions(HTMLNodeParser.ParserFlags.HtmlElementClassName, "list-Floors")),
                ["location"] = Tuple.Create("h3", new HTMLNodeParser.ParseOptions(HTMLNodeParser.ParserFlags.HtmlElementClassName, "")),
                ["url"] = Tuple.Create("a", new HTMLNodeParser.ParseOptions(HTMLNodeParser.ParserFlags.Hyperlink, "")),
                ["img"] = Tuple.Create("img", new HTMLNodeParser.ParseOptions(HTMLNodeParser.ParserFlags.Image, "---none")),

            };
            List<Dictionary<string, string>> collectedData = HTMLNodeParser.FeedHTML(wd.PageSource, "tr", "list-row", rawValues);

            foreach (Dictionary<string,string> entry in collectedData)
            {
                var titleAndDescription = deepScrap(wd, entry["url"]);
                entry["title"] = titleAndDescription["title"];
                entry["description"] = titleAndDescription["description"];
            }
            
            List<HousingObject> databaseEntries = new List<HousingObject>();
            foreach (var entry in collectedData)
            {
                HousingObject obj = toDBO(entry);
                databaseEntries.Add(obj);
            }

            //remove dublicates from UNIQUE keys before DB inserts
            databaseEntries = databaseEntries
                .GroupBy(entry => entry.url)
                .Select(g => g.First())
                .ToList();
            
            PWDatabaseInitializer.InsertMany(dbc, databaseEntries);
            wd.Close();
            return databaseEntries.ToArray();
        }

        //atidaro Aruodas.lt skelbimo puslapi ir paima duomenis is vidaus
        public static Dictionary<string, string> deepScrap(WebDriver wd, string url)
        {
            ConsoleWriter.WriteHttpGetScrappers(url);
            wd.Navigate().GoToUrl(url);
            Dictionary<string, Tuple<string, HTMLNodeParser.ParseOptions>> rawValues =
                new Dictionary<string, Tuple<string, HTMLNodeParser.ParseOptions>>
                {
                    ["title"] = Tuple.Create("h1", new HTMLNodeParser.ParseOptions(HTMLNodeParser.ParserFlags.HtmlElementClassName, "obj-header-text")),
                    ["description"] = Tuple.Create("div", new HTMLNodeParser.ParseOptions(HTMLNodeParser.ParserFlags.HtmlElementId, "collapsedText"))
                };
            List<Dictionary<string, string>> collectedData = HTMLNodeParser.FeedHTML(wd.PageSource, "div", "obj-cont", rawValues);
            return collectedData.First();
        }

        public HousingObject toDBO(Dictionary<string, string> insertable)
        {
            //parse price
            string price = insertable["price"].Replace(" ", "").Replace("\n", "").Replace("\r", "");
            string currency = price.Substring(price.Length - 1); //last character

            int priceAmount = Int32.Parse(new string(price.Where(c => char.IsDigit(c)).ToArray()));
            
            //parse floors
            var floors = insertable["floors"].Replace(" ", "").Replace("\n", "").Replace("\r", "").Split("/");
            int currentFloor = Int32.Parse(floors[0]);
            int maxFloor = Int32.Parse(floors[1]);
            
            //parse rooms and area
            var rooms = Int32.Parse(insertable["rooms"].Replace(" ", "").Replace("\n", "").Replace("\r", ""));
            var area = (int) float.Parse(insertable["area"].Replace(" ", "").Replace("\n", "").Replace("\r", "")
                , CultureInfo.InvariantCulture);

            //parse location
            var location = insertable["location"].Replace("\n", " ").Replace("\r", " ");
            HousingObject dbObject = new HousingObject
            {
                Source_id = 1,
                title = insertable["title"],
                url = insertable["url"],
                price = priceAmount,
                location = location.Trim(),
                timestamp = new DateTimeOffset(DateTime.UtcNow).ToUnixTimeSeconds(),
                Currency = currency,
                area = area,
                rooms = Int32.Parse(insertable["rooms"].Trim()),
                floorsMax = maxFloor,
                floorsThis = currentFloor,
                description = insertable["description"],
                imgUrl = insertable["img"]
            };
            return dbObject;
        }
    }
}