domains
    name = symbol
    subject = symbol
    mark = integer
    class_num = integer
    gender = symbol
    namelist = name*
    subjectlist = subject*
    classlist = class_num*

database
    student(name, name, gender, class_num)
    grade(name, subject, mark)
    participation_race(name)
    participation_beauty(name)
    participation_olymp(name, subject, class_num)
    judge(name)
    temp_name(name)
    temp_class(class_num)
    temp_subject(subject)

predicates
    nondeterm menu
    nondeterm check_menu(symbol)
    nondeterm add_student
    nondeterm add_grade
    nondeterm check_prize
    nondeterm win_prize
    nondeterm has_disorders
    nondeterm disorder_in_class(class_num)
    nondeterm count_lazy(integer)
    nondeterm is_lazy(name)
    nondeterm olympiads_held(integer)
    nondeterm olympiad_held(subject)
    nondeterm races_held
    nondeterm beauty_held
    nondeterm count_class_students(class_num, integer)
    nondeterm count_excellent_in_class(class_num, integer)
    nondeterm save_data
    nondeterm load_data
    nondeterm member_name(name, namelist)
    nondeterm length_namelist(namelist, integer)
    nondeterm length_subjectlist(subjectlist, integer)
    nondeterm length_classlist(classlist, integer)
    nondeterm clear_temp_data
    nondeterm collect_names(namelist)
    nondeterm collect_classes(classlist)
    nondeterm collect_subjects(subjectlist)
    nondeterm check_all_classes(classlist, subject)
    nondeterm count_olymp_in_class(class_num, subject, integer)
    nondeterm check_race_members(namelist)
    nondeterm check_all_race_grades(namelist)
    nondeterm check_beauty_members(namelist)
    nondeterm check_all_female(namelist)
    nondeterm check_no_physics(namelist)
    nondeterm count_judges(integer)
    nondeterm check_all_judges
    nondeterm count_judge_olymps(name, integer)
    nondeterm findall_temp(namelist)
    nondeterm findall_temp_class(classlist)
    nondeterm findall_temp_subject(subjectlist)

clauses
    menu:-
        write("\n=== SCHOOL PRIZE SYSTEM ==="),
        write("\n1. Load database"),
        write("\n2. Add student"),
        write("\n3. Add grade"),
        write("\n4. Check Prize"),
        write("\n5. Save and Exit"),
        write("\nSelect: "),
        readln(MenuStr),
        check_menu(MenuStr).
        
    check_menu("1"):- load_data, menu.
    check_menu("2"):- add_student, menu.
    check_menu("3"):- add_grade, menu.
    check_menu("4"):- check_prize, menu.
    check_menu("5"):- save_data, write("\nBye!"), !.
    check_menu(_):- write("\nInvalid"), menu.

    add_student:-
        write("\nName: "), readln(Name),
        write("Surname: "), readln(Surname),
        write("Gender (male/female): "), readln(Gender),
        write("Class: "), readint(Class),
        assertz(student(Name, Surname, Gender, Class)),
        write("Added.").

    add_grade:-
        write("\nStudent Name: "), readln(Name),
        write("Subject: "), readln(Subject),
        write("Mark (2-5): "), readint(Mark),
        assertz(grade(Name, Subject, Mark)),
        write("Added.").

    check_prize:-
        write("\n--- Analysis ---"),
        win_prize,
        write("\n*** SCHOOL WINS! ***"), !.
    check_prize:-
        write("\n--- School loses ---").

    win_prize:-
        olympiads_held(CountO),
        CountO >= 3,
        write("\n[OK] Olympiads: "), write(CountO),
        beauty_held,
        write("\n[OK] Beauty contest"),
        races_held,
        write("\n[OK] Races"),
        not(has_disorders),
        write("\n[OK] No disorders"),
        count_lazy(CountL),
        CountL <= 3,
        write("\n[OK] Lazy: "), write(CountL).

    is_lazy(Name):- grade(Name, _, 2).
    
    count_lazy(Count):-
        clear_temp_data,
        is_lazy(N), assertz(temp_name(N)), fail;
        collect_names(List), length_namelist(List, Count).

    disorder_in_class(Class):- 
    	student(_, _, _, Class),           % This line "binds" Class to an existing class number
    	count_class_students(Class, CountS), 
    	CountS > 4, 
    	count_excellent_in_class(Class, CountE), 
    	CountE = 0.

    has_disorders:- disorder_in_class(_).

    count_class_students(Class, Count):-
        clear_temp_data,
        student(S, _, _, Class), assertz(temp_name(S)), fail;
        collect_names(List), length_namelist(List, Count).

    count_excellent_in_class(Class, Count):-
        clear_temp_data,
        student(Name, _, _, Class), grade(Name, _, 5), assertz(temp_name(Name)), fail;
        collect_names(List), length_namelist(List, Count).

    olympiad_held(Subject):-
        clear_temp_data,
        participation_olymp(_, Subject, C), assertz(temp_class(C)), fail;
        collect_classes(Classes),
        check_all_classes(Classes, Subject).

    check_all_classes([], _).
    check_all_classes([C|Rest], Subject):-
        count_olymp_in_class(C, Subject, Count),
        Count >= 4,
        check_all_classes(Rest, Subject).

    count_olymp_in_class(Class, Subject, Count):-
        clear_temp_data,
        participation_olymp(S, Subject, Class), assertz(temp_name(S)), fail;
        collect_names(List), length_namelist(List, Count).

    olympiads_held(Count):-
        clear_temp_data,
        participation_olymp(_, Subj, _),
        olympiad_held(Subj),
        assertz(temp_subject(Subj)), fail;
        collect_subjects(List), length_subjectlist(List, Count).

    races_held:-
        clear_temp_data,
        participation_race(P), assertz(temp_name(P)), fail;
        collect_names(Racers),
        length_namelist(Racers, Count),
        Count >= 2,
        check_race_members(Racers).

    check_race_members(Racers):-
        member_name(Male, Racers), student(Male, _, male, _),
        member_name(Fem, Racers), student(Fem, _, female, _),
        check_all_race_grades(Racers).

    check_all_race_grades([]).
    check_all_race_grades([P|Rest]):-
        grade(P, _, 3),
        check_all_race_grades(Rest).

    beauty_held:-
        clear_temp_data,
        participation_beauty(P), assertz(temp_name(P)), fail;
        collect_names(Beauties),
        length_namelist(Beauties, CountB),
        CountB >= 2,
        check_beauty_members(Beauties).

    check_beauty_members(Beauties):-
        check_all_female(Beauties),
        check_no_physics(Beauties),
        count_judges(CountJ),
        CountJ = 3,
        check_all_judges.

    check_all_female([]).
    check_all_female([P|Rest]):-
        student(P, _, female, _),
        check_all_female(Rest).

    check_no_physics([]).
    check_no_physics([P|Rest]):-
        not(participation_olymp(P, physics, _)),
        check_no_physics(Rest).

    count_judges(Count):-
        clear_temp_data,
        judge(J), assertz(temp_name(J)), fail;
        collect_names(List), length_namelist(List, Count).

    check_all_judges:-
        judge(J),
        count_judge_olymps(J, Count),
        Count <= 1,
        fail;
        true.

    count_judge_olymps(Judge, Count):-
        clear_temp_data,
        participation_olymp(Judge, _, _), assertz(temp_name(Judge)), fail;
        collect_names(List), length_namelist(List, Count).

    load_data:-
        consult("school", dbasedom),
        write("\nLoaded.").
    
    save_data:-
        save("school", dbasedom).

    clear_temp_data:-
        retract(temp_name(_)), fail.
    clear_temp_data:-
        retract(temp_class(_)), fail.
    clear_temp_data:-
        retract(temp_subject(_)), fail.
    clear_temp_data.

    collect_names(List):-
        findall_temp(List).

    findall_temp([]):-
        not(temp_name(_)).
    findall_temp([H|T]):-
        temp_name(H),
        retract(temp_name(H)),
        findall_temp(T).

    collect_classes(List):-
        findall_temp_class(List).

    findall_temp_class([]):-
        not(temp_class(_)).
    findall_temp_class([H|T]):-
        temp_class(H),
        retract(temp_class(H)),
        findall_temp_class(T).

    collect_subjects(List):-
        findall_temp_subject(List).

    findall_temp_subject([]):-
        not(temp_subject(_)).
    findall_temp_subject([H|T]):-
        temp_subject(H),
        retract(temp_subject(H)),
        findall_temp_subject(T).

    member_name(X, [X|_]).
    member_name(X, [_|T]):- member_name(X, T).

    length_namelist([], 0).
    length_namelist([_|T], N):- length_namelist(T, N1), N = N1 + 1.

    length_subjectlist([], 0).
    length_subjectlist([_|T], N):- length_subjectlist(T, N1), N = N1 + 1.

    length_classlist([], 0).
    length_classlist([_|T], N):- length_classlist(T, N1), N = N1 + 1.

goal
    menu.
