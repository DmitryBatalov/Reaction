# Operators

The following parameterized async observerable returning functions (operators) are
currently supported. Other operators may be implemented on-demand, but the goal is to keep it simple
and not make this into a full featured [ReactiveX](http://reactivex.io/) implementation (if possible).

## Create

Functions for creating (`'a -> IAsyncObservable<'a>`) an async observable.

- **empty** : unit -> IAsyncObservable<'a>
- **single** : 'a -> IAsyncObservable<'a>
- **fail** : exn -> IAsyncObservable<'a>
- **defer** : (unit -> IAsyncObservable<'a>) -> IAsyncObservable<'a>
- **create** : (AsyncObserver\<'a\> -> Async\<AsyncDisposable\>) -> IAsyncObservable<'a>
- **ofSeq** : seq<'a> -> IAsyncObservable<'a>
- **ofAsyncSeq** : AsyncSeq<'a> -> IAsyncObservable<'a> *(Not available in Fable)*
- **timer** : int -> IAsyncObservable\<int\>
- **interval** int -> IAsyncObservable\<int\>

## Transform

Functions for transforming (`IAsyncObservable<'a> -> IAsyncObservable<'b>`) an async observable.

- **map** : ('a -> 'b) -> IAsyncObservable<'a> -> IAsyncObservable<'b>
- **mapi** : ('a*int -> 'b) -> IAsyncObservable<'a> -> IAsyncObservable<'b>
- **mapAsync** : ('a -> Async<'b>) -> IAsyncObservable<'a> -> IAsyncObservable<'b>
- **mapiAsync** : ('a*int -> Async<'b>) -> IAsyncObservable<'a> -> IAsyncObservable<'b>
- **flatMap** : ('a -> IAsyncObservable<'b>) -> IAsyncObservable<'a> -> IAsyncObservable<'b>
- **flatMapi** : ('a*int -> IAsyncObservable<'b>) -> IAsyncObservable<'a> -> IAsyncObservable<'b>
- **flatMapAsync** : ('a -> Async\<IAsyncObservable\<'b\>\>) -> IAsyncObservable<'a> -> IAsyncObservable<'b>
- **flatMapiAsync** : ('a*int -> Async\<IAsyncObservable\<'b\>\>) -> IAsyncObservable<'a> -> IAsyncObservable<'b>
- **flatMapLatest** : ('a -> IAsyncObservable<'b>) -> IAsyncObservable<'a> -> IAsyncObservable<'b>
- **flatMapLatestAsync** : ('a -> Async\<IAsyncObservable\<'b\>\>) -> IAsyncObservable<'a> -> IAsyncObservable<'b>
- **catch** : (exn -> IAsyncObservable<'a>) -> IAsyncObservable<'a> -> IAsyncObservable<'a>

## Filter

Functions for filtering (`IAsyncObservable<'a> -> IAsyncObservable<'a>`) an async observable.

- **filter** : ('a -> bool) -> IAsyncObservable<'a> -> IAsyncObservable<'a>
- **filterAsync** : ('a -> Async\<bool\>) -> IAsyncObservable<'a> -> IAsyncObservable<'a>
- **distinctUntilChanged** : IAsyncObservable<'a> -> IAsyncObservable<'a>
- **takeUntil** : IAsyncObservable<'b> -> IAsyncObservable<'a> -> IAsyncObservable<'a>
- **choose** : ('a -> 'b option) -> IAsyncObservable<'a> -> IAsyncObservable<'b>
- **chooseAsync** : ('a -> Async<'b option>) -> IAsyncObservable<'a> -> IAsyncObservable<'b>

## Aggregate

- **scan** : 's -> ('s -> 'a -> 's) -> IAsyncObservable<'a> -> IAsyncObservable<'s>
- **scanAsync** : 's -> ('s -> 'a -> Async<'s>) -> IAsyncObservable<'a> -> IAsyncObservable<'s>
- **groupBy** : ('a -> 'g) -> IAsyncObservable<'a> -> IAsyncObservable\<IAsyncObservable\<'a\>\>

## Combine

Functions for combining multiple async observables into one.

- **merge** : IAsyncObservable<'a> -> IAsyncObservable<'a> -> IAsyncObservable<'a>
- **mergeInner** : IAsyncObservable<IAsyncObservable<'a>> -> IAsyncObservable<'a>
- **switchLatest** : IAsyncObservable<IAsyncObservable<'a>> -> IAsyncObservable<'a>
- **concat** : seq<IAsyncObservable<'a>> -> IAsyncObservable<'a>
- **startWith** : seq<'a> -> IAsyncObservable<'a> -> IAsyncObservable<'a>
- **combineLatest** : IAsyncObservable<'b> -> IAsyncObservable<'a> -> IAsyncObservable<'a*'b>
- **withLatestFrom** : IAsyncObservable<'b> -> IAsyncObservable<'a> -> IAsyncObservable<'a*'b>
- **zipSeq** : seq<'b> -> IAsyncObservable<'a> -> IAsyncObservable<'a*'b>

## Time-shift

Functions for time-shifting (`IAsyncObservable<'a> -> IAsyncObservable<'a>`) an async observable.

- **delay** : int -> IAsyncObservable<'a> -> IAsyncObservable<'a>
- **debounce** : int -> IAsyncObservable<'a> -> IAsyncObservable<'a>

## Leave

Functions for leaving (`IAsyncObservable<'a> -> 'a`) the async observable.

-- **toAsyncSeq** : IAsyncObservable<'a> -> AsyncSeq<'a> *(Not available in Fable)*
