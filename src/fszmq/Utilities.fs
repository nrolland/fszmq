﻿(* ------------------------------------------------------------------------
This file is part of fszmq.

fszmq is free software: you can redistribute it and/or modify
it under the terms of the GNU Lesser General Public License as published 
by the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

fszmq is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
GNU Lesser General Public License for more details.

You should have received a copy of the GNU Lesser General Public License
along with fszmq. If not, see <http://www.gnu.org/licenses/>.

Copyright (c) 2011-2013 Paulmichael Blasucci
------------------------------------------------------------------------ *)
namespace fszmq

open System
open System.Collections.Generic
open System.Runtime.CompilerServices
open System.Text

/// Contains methods for working with ZMQ's proxying capabilities
[<CompilationRepresentation(CompilationRepresentationFlags.ModuleSuffix)>]
module Proxying =

  /// creates a proxy connection passing messages between two sockets, 
  /// with an (optional) third socket for supplemental data capture 
  [<CompiledName("Proxy")>]
  let proxy (frontend:Socket) (backend:Socket) (capture:Socket option) =
    match capture with
    | Some capture -> C.zmq_proxy(frontend.Handle,backend.Handle,capture.Handle)
    | _            -> C.zmq_proxy(frontend.Handle,backend.Handle,            0n)

/// Utilities for working with Polling from languages other than F#
[<Extension>]
type ProxyingExtensions =
  
  /// creates a proxy connection passing messages between two sockets
  [<Extension>]
  static member Proxy(frontend,backend) = Proxying.proxy frontend backend None

  /// creates a proxy connection passing messages between two sockets, 
  /// with an third socket for supplemental data capture (e.g. logging)
  [<Extension>]
  static member Proxy(frontend,backend,capture) = Proxying.proxy frontend backend (Some capture)

[<RequireQualifiedAccess>]
module Z85 =
    
  let [<Literal>] private KEY_SIZE = 41 //TODO: should this be hard-coded?

  [<CompiledName("MakeCurveKeyPair")>]
  let curveKeyPair () = 
    let publicKey,secretKey = StringBuilder(KEY_SIZE),StringBuilder(KEY_SIZE)
    if C.zmq_curve_keypair(publicKey,secretKey) <> 0 then ZMQ.error()
    publicKey,secretKey

  [<CompiledName("Encode")>]
  let encode data =
    let datalen = Array.length data // size must be divisible by 4
    let buffer  = StringBuilder (datalen * 5 / 4 + 1)
    if C.zmq_z85_encode(buffer,data,unativeint datalen) = 0n then ZMQ.error()
    string buffer

  [<CompiledName("Decode")>]
  let decode data =
    let datalen = String.length data // size must be divisible by 5
    let buffer  = Array.zeroCreate (datalen * 4 / 5)
    if C.zmq_z85_decode(buffer,data) = 0n then ZMQ.error()
    buffer
