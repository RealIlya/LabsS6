domains
    list_i = integer* /* Domain of integer lists */
predicates
    /* Declarative meaning: true if List2 is obtained by inserting X before every occurrence of Y in List1 */
    insert_before_all(integer, integer, list_i, list_i)
    
    /* Declarative meaning: true if List2 contains X1 and X2 inserted before the last element of List1 */
    insert_before_last(integer, integer, list_i, list_i)
    
    /* Declarative meaning: true if List2 is a sorted list that includes X */
    insert_sorted(integer, list_i, list_i)
clauses
    /* a) Insert before all occurrences of Y */
    insert_before_all(_, _, [], []).
    insert_before_all(X, Y, [Y|Tail], [X, Y|ResultTail]) :-
        !, insert_before_all(X, Y, Tail, ResultTail).
    insert_before_all(X, Y, [H|T], [H|ResultTail]) :-
        insert_before_all(X, Y, T, ResultTail).
    /* b) Insert before the last element */
    insert_before_last(X1, X2, [Last], [X1, X2, Last]) :- !.
    insert_before_last(X1, X2, [H|T], [H|ResultTail]) :-
        insert_before_last(X1, X2, T, ResultTail).
    /* c) Insert into a sorted list */
    insert_sorted(X, [], [X]).
    insert_sorted(X, [H|T], [X, H|T]) :- X <= H, !.
    insert_sorted(X, [H|T], [H|ResultTail]) :-
        insert_sorted(X, T, ResultTail).
        
    goal
    /* Tests for insert_before_all (insert 0 before every 1) */
    write("--- insert_before_all ---"), nl,
    insert_before_all(0, 1, [1, 2, 1, 3], L1), 
    write("Test 1 (middle): ", L1), nl, % Expected result [0, 1, 2, 0, 1, 3]
    insert_before_all(0, 1, [1, 1], L2), 
    write("Test 2 (only target): ", L2), nl, % Expected result [0, 1, 0, 1]
    insert_before_all(0, 1, [2, 3], L3), 
    write("Test 3 (no target): ", L3), nl, % Expected result [2, 3]
    insert_before_all(0, 1, [], L4), 
    write("Test 4 (empty): ", L4), nl, nl, % Expected result []
    /* Tests for insert_before_last (insert 7, 8 before the last element) */
    write("--- insert_before_last ---"), nl,
    insert_before_last(7, 8, [1, 2, 3], L5), 
    write("Test 5 (standard): ", L5), nl, % Expected result [1, 2, 7, 8, 3]
    insert_before_last(7, 8, [5], L6), 
    write("Test 6 (single element): ", L6), nl, nl, % Expected result [7, 8, 5]
    /* Tests for insert_sorted (insert into a sorted list) */
    write("--- insert_sorted ---"), nl,
    insert_sorted(5, [2, 4, 6, 8], L7), 
    write("Test 7 (middle): ", L7), nl, % Expected result [2, 4, 5, 6, 8]
    insert_sorted(1, [2, 3], L8), 
    write("Test 8 (start): ", L8), nl, % Expected result [1, 2, 3]
    insert_sorted(10, [2, 3], L9), 
    write("Test 9 (end): ", L9), nl, % Expected result [2, 3, 10]
    insert_sorted(5, [], L10), 
    write("Test 10 (empty): ", L10), nl. % Expected result [5]
