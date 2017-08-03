#r "./packages/Suave/lib/net40/Suave.dll"
#r "./packages/FAKE/tools/FakeLib.dll"
#r "./packages/FSharp.Data/lib/net40/FSharp.Data.dll"
#r "./packages/Newtonsoft.Json/lib/net40/Newtonsoft.Json.dll"
#r "./packages/Fable.Remoting.Suave/lib/Fable.Remoting.Suave.dll"

open Suave
open Suave.Http
open Suave.Filters
open Suave.Operators
open Fable.Remoting.Suave
open System
open FSharp.Data


type Silver = JsonProvider<"""./data/SILVER.json""">
type Gold = JsonProvider<"""./data/GOLD.json""">

let silvers = Silver.GetSample()
let golds = Gold.GetSample()

let SilverData = silvers.Data
let GoldData = golds.Data


type MetalPrices = {
     Metal: String
     Days: DateTime List
     Price: Decimal List
    }

let SilverInfos() = {
  Metal = "Silver"
  Days  = [ for each in SilverData -> each.DateTime ]
  Price = [ for each in SilverData -> each.Numbers.[0] ];
}

let GoldInfos() = {
  Metal = "Gold"
  Days  = [ for each2 in GoldData -> each2.DateTime ]
  Price = [ for each2 in GoldData -> each2.Numbers.[0] ];
}



type IServer = {
    getSilverPricesUSD : unit -> Async<MetalPrices>
    getGoldPricesUSD : unit -> Async<MetalPrices>
}



let server : IServer = {
    getSilverPricesUSD = fun () -> async {return SilverInfos()}
    getGoldPricesUSD = fun () -> async {return GoldInfos()}

}


let routeBuilder typeName methodName = 
        sprintf "/api/%s/%s" typeName methodName

    // enable logging to console
FableSuaveAdapter.logger <- Some Console.WriteLine
    // create the webpart with route builder


// create routes from the implementation
// webApp : WebPart
let webApp = FableSuaveAdapter.webPartWithBuilderFor server routeBuilder 
// Run server

let myCfg =
  { defaultConfig with
      bindings = [ HttpBinding.createSimple HTTP "127.0.0.1" 8083 ]
    }
startWebServer myCfg webApp