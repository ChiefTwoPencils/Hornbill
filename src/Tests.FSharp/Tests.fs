﻿module Tests

open Hornbill.FSharp
open System.Net.Http
open System.Net
open Hornbill
open System.Threading

let createFakeService() = 
  let fakeService = new FakeService()
  fakeService.Start() |> ignore
  let httpClient = new HttpClient(BaseAddress = fakeService.Uri)
  fakeService, httpClient

[<Test>]
let body() = 
  let fakeService, httpClient = createFakeService()
  Response.withBody 200 "foo" |> fakeService.AddResponse "/foo" Method.GET
  httpClient.GetStringAsync("/foo").Result == "foo"
  fakeService.Stop()

[<Test>]
let headers() = 
  let fakeService, httpClient = createFakeService()
  Response.withHeaders 200 [ "foo", "bar" ] |> fakeService.AddResponse "/foo" Method.GET
  let response = httpClient.GetAsync("/foo").Result
  (response.Headers |> Seq.head).Key == "foo"
  (response.Headers |> Seq.head).Value == [| "bar" |]
  fakeService.Stop()

[<Test>]
let dlg() =
  let fakeService, httpClient = createFakeService()
  Response.withDelegate (fun x -> Response.withStatusCode 200) |> fakeService.AddResponse "/foo" Method.GET
  httpClient.GetAsync("/foo").Result.StatusCode == HttpStatusCode.OK

[<Test>]
let evnt() =
  let fakeService, httpClient = createFakeService()
  Response.WithStatusCode 200 |> fakeService.AddResponse "/foo" Method.GET
  let autoResetEvent = new AutoResetEvent false
  fakeService.RequestReceived.Add(fun x -> if x.Request.Path = "/foo" then autoResetEvent.Set() |> ignore)
  httpClient.GetAsync("/foo").Result.StatusCode == HttpStatusCode.OK
  autoResetEvent.WaitOne 1000 == true