predicates
    nondeterm father(symbol, symbol)
    nondeterm mother(symbol, symbol)
    nondeterm man(symbol)
    nondeterm woman(symbol)
    nondeterm sister(symbol, symbol)
    nondeterm brother(symbol, symbol)
    nondeterm son(symbol, symbol)
    nondeterm daughter(symbol, symbol)
    nondeterm husband(symbol, symbol)
    nondeterm wife(symbol, symbol)
    nondeterm cousin_female(symbol, symbol)
    nondeterm cousin_male(symbol, symbol)
    nondeterm uncle(symbol, symbol)
    nondeterm aunt(symbol, symbol)
    nondeterm nephew(symbol, symbol)
    nondeterm niece(symbol, symbol)
    nondeterm grandfather(symbol, symbol)
    nondeterm grandmother(symbol, symbol)
    nondeterm great_grandfather(symbol, symbol)
    nondeterm great_grandmother(symbol, symbol)
    nondeterm grandson(symbol, symbol)
    nondeterm granddaughter(symbol, symbol)
    nondeterm great_grandson(symbol, symbol)
    nondeterm great_granddaughter(symbol, symbol)
    nondeterm parent(symbol, symbol)
    nondeterm happy(symbol)
    nondeterm has_two_children(symbol)
    nondeterm check_happy(symbol)
    nondeterm check_two(symbol)


clauses
    sister(X, Y):- mother(M, X), mother(M, Y), father(F, X), father(F, Y), X<>Y, woman(X).
    brother(X, Y):- mother(M, X), mother(M, Y), father(F, X), father(F, Y), X<>Y, man(X).
    
    son(X, Y):- father(Y, X), man(X).
    son(X, Y):- mother(Y, X), man(X).
    
    daughter(X, Y):- father(Y, X), woman(X).
    daughter(X, Y):- mother(Y, X), woman(X).

    husband(X, Y):- father(X, C), mother(Y, C), man(X).
    wife(X, Y):- father(Y, C), mother(X, C), woman(X).

    cousin_female(X, Y):- father(FX, X), father(FY, Y), brother(FX, FY), X<>Y, woman(X).
    cousin_female(X, Y):- mother(MX, X), mother(MY, Y), sister(MX, MY), X<>Y, woman(X).
    cousin_female(X, Y):- father(FX, X), mother(MY, Y), brother(FX, MY), X<>Y, woman(X).
    cousin_female(X, Y):- mother(MX, X), father(FY, Y), sister(MX, FY), X<>Y, woman(X).

    cousin_male(X, Y):- father(FX, X), father(FY, Y), brother(FX, FY), X<>Y, man(X).
    cousin_male(X, Y):- mother(MX, X), mother(MY, Y), sister(MX, MY), X<>Y, man(X).
    cousin_male(X, Y):- father(FX, X), mother(MY, Y), brother(FX, MY), X<>Y, man(X).
    cousin_male(X, Y):- mother(MX, X), father(FY, Y), sister(MX, FY), X<>Y, man(X).

    uncle(X, Y):- brother(X, P), father(P, Y), man(X).
    uncle(X, Y):- brother(X, P), mother(P, Y), man(X).
    uncle(X, Y):- husband(X, A), sister(A, P), father(P, Y), man(X).
    uncle(X, Y):- husband(X, A), sister(A, P), mother(P, Y), man(X).

    aunt(X, Y):- sister(X, P), father(P, Y), woman(X).
    aunt(X, Y):- sister(X, P), mother(P, Y), woman(X).
    aunt(X, Y):- wife(X, U), brother(U, P), father(P, Y), woman(X).
    aunt(X, Y):- wife(X, U), brother(U, P), mother(P, Y), woman(X).

    nephew(X, Y):- brother(Y, P), father(P, X), man(X).
    nephew(X, Y):- brother(Y, P), mother(P, X), man(X).
    nephew(X, Y):- sister(Y, P), father(P, X), man(X).
    nephew(X, Y):- sister(Y, P), mother(P, X), man(X).

    niece(X, Y):- brother(Y, P), father(P, X), woman(X).
    niece(X, Y):- brother(Y, P), mother(P, X), woman(X).
    niece(X, Y):- sister(Y, P), father(P, X), woman(X).
    niece(X, Y):- sister(Y, P), mother(P, X), woman(X).

    grandfather(X, Y):- father(X, Z), father(Z, Y), man(X).
    grandfather(X, Y):- father(X, Z), mother(Z, Y), man(X).

    grandmother(X, Y):- mother(X, Z), father(Z, Y), woman(X).
    grandmother(X, Y):- mother(X, Z), mother(Z, Y), woman(X).

    great_grandfather(X, Y):- father(X, Z), grandfather(Z, Y), man(X).
    great_grandfather(X, Y):- father(X, Z), grandmother(Z, Y), man(X).

    great_grandmother(X, Y):- mother(X, Z), grandfather(Z, Y), woman(X).
    great_grandmother(X, Y):- mother(X, Z), grandmother(Z, Y), woman(X).

    grandson(X, Y):- father(Y, Z), father(Z, X), man(X).
    grandson(X, Y):- father(Y, Z), mother(Z, X), man(X).
    grandson(X, Y):- mother(Y, Z), father(Z, X), man(X).
    grandson(X, Y):- mother(Y, Z), mother(Z, X), man(X).

    granddaughter(X, Y):- father(Y, Z), father(Z, X), woman(X).
    granddaughter(X, Y):- father(Y, Z), mother(Z, X), woman(X).
    granddaughter(X, Y):- mother(Y, Z), father(Z, X), woman(X).
    granddaughter(X, Y):- mother(Y, Z), mother(Z, X), woman(X).

    great_grandson(X, Y):- great_grandfather(Y, X), man(X).
    great_grandson(X, Y):- great_grandmother(Y, X), man(X).

    great_granddaughter(X, Y):- great_grandfather(Y, X), woman(X).
    great_granddaughter(X, Y):- great_grandmother(Y, X), woman(X).

    man(petr).           
    man(ivan).           
    man(matvey).       
    man(alexey).        
    man(dmitry).         
    man(andrey).         
    man(sergey).         

    woman(ekaterina).  
    woman(sofia).        
    woman(olga).         
    woman(elena).        
    woman(anna).         

    father(petr, ivan).           
    father(petr, matvey).       
    father(ivan, alexey).        
    father(ivan, dmitry).         
    father(alexey, andrey).      
    father(alexey, anna).         
    father(dmitry, sergey).       

    mother(ekaterina, ivan). 
    mother(ekaterina, matvey). 
    mother(sofia, alexey).       
    mother(sofia, dmitry).        
    mother(olga, andrey).        
    mother(olga, anna).           
    mother(elena, sergey).        

    parent(X,Y):- father(X,Y).
    parent(X,Y):- mother(X,Y).

    happy(X):- 
        man(X), check_happy(X);
        woman(X), check_happy(X).
       
    check_happy(X) :-
         parent(X, _),
         !.
    
    has_two_children(X):-
        man(X), check_two(X);
        woman(X), check_two(X).

    check_two(P) :- parent(P, C), sister(C, _), !.
    check_two(P) :- parent(P, C), brother(C, _), !.

goal
    son(andrey, X).
    % daughter(anna, X).
    % grandson(andrey, X).
    %happy(X).
    %has_two_children(X).