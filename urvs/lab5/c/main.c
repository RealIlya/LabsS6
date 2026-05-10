#define _POSIX_C_SOURCE 200809L

#include <errno.h>
#include <limits.h>
#include <signal.h>
#include <stdio.h>
#include <stdlib.h>
#include <string.h>
#include <sys/ipc.h>
#include <sys/sem.h>
#include <sys/shm.h>
#include <sys/types.h>
#include <sys/wait.h>
#include <time.h>
#include <unistd.h>

#define CHILD_COUNT 2
#define LINES_PER_STANZA 4
#define MAX_STANZAS 8
#define LINE_SIZE 128
#define DEFAULT_DELAY_ONE 120
#define DEFAULT_DELAY_TWO 210

#define SEM_EMPTY 0
#define SEM_FULL 1
#define SEM_MUTEX 2

union semun {
    int val;
    struct semid_ds *buf;
    unsigned short *array;
};

struct stanza_message {
    int stanza_no;
    int sender_no;
    int finished;
    char lines[LINES_PER_STANZA][LINE_SIZE];
};

static const char *poem[MAX_STANZAS][LINES_PER_STANZA] = {
    {
        "Над тихой улицей закат,",
        "Скользит по стеклам теплым светом,",
        "И первый фонарей отряд",
        "Уже вступает в город следом."
    },
    {
        "На перекрестке меркнет шум,",
        "Трамвай уходит к повороту,",
        "И вечер, сбросив тяжесть дум,",
        "Ведет дома к ночной дремоте."
    },
    {
        "Река под мостом темна,",
        "Но в ней дрожат огни витрины,",
        "И смотрит в воду тишина",
        "С высокого речного склона."
    },
    {
        "Во двор спускается прохлада,",
        "Листва шуршит у старых стен,",
        "И поздний ветер до заката",
        "Хранит размеренный обмен."
    },
    {
        "На лавке забывают зонт,",
        "В окне качнулся отблеск синий,",
        "И кто-то, выйдя на балкон,",
        "Стоит над улицей пустынной."
    },
    {
        "Уже почти уснули крыши,",
        "Стал редким шаг по мостовой,",
        "И только башенные ниши",
        "Звучат отбойною волной."
    },
    {
        "Ночь собирает свет в кольцо,",
        "Стирая суету квартала,",
        "И город прячет под крыльцо",
        "Все то, что день не досказал нам."
    },
    {
        "А утром заново начнется",
        "Движенье улиц и людей,",
        "Но в памяти еще вернется",
        "Спокойный ритм ночных огней."
    }
};

static int parse_positive_int(const char *s, int *out) {
    char *end;
    long value;

    errno = 0;
    value = strtol(s, &end, 10);
    if (*end != '\0' || value <= 0 || value > INT_MAX || errno == ERANGE) {
        return 1;
    }

    *out = (int)value;
    return 0;
}

static int sem_change(int semid, unsigned short sem_num, short sem_delta) {
    struct sembuf op;

    op.sem_num = sem_num;
    op.sem_op = sem_delta;
    op.sem_flg = 0;

    if (semop(semid, &op, 1) < 0) {
        return -1;
    }
    return 0;
}

static void sleep_ms(long ms) {
    struct timespec ts;

    ts.tv_sec = ms / 1000;
    ts.tv_nsec = (ms % 1000) * 1000000L;
    nanosleep(&ts, NULL);
}

static void cleanup_ipc(int shmid, int semid, struct stanza_message *shared) {
    if (shared != (void *)-1) {
        shmdt(shared);
    }
    if (shmid >= 0) {
        shmctl(shmid, IPC_RMID, NULL);
    }
    if (semid >= 0) {
        semctl(semid, 0, IPC_RMID);
    }
}

static void child_work(int child_no, int semid, struct stanza_message *shared, int delay_ms) {
    int stanza_index;
    int i;

    for (stanza_index = child_no - 1; stanza_index < MAX_STANZAS; stanza_index += CHILD_COUNT) {
        if (sem_change(semid, SEM_EMPTY, -1) < 0 || sem_change(semid, SEM_MUTEX, -1) < 0) {
            fprintf(stderr, "error: child %d semop failed: %s\n", child_no, strerror(errno));
            _exit(1);
        }

        shared->stanza_no = stanza_index + 1;
        shared->sender_no = child_no;
        shared->finished = 0;
        for (i = 0; i < LINES_PER_STANZA; i++) {
            snprintf(shared->lines[i], LINE_SIZE, "%s", poem[stanza_index][i]);
        }

        printf("P%d(pid=%ld): sent stanza %d\n", child_no, (long)getpid(), shared->stanza_no);
        fflush(stdout);

        if (sem_change(semid, SEM_MUTEX, 1) < 0 || sem_change(semid, SEM_FULL, 1) < 0) {
            fprintf(stderr, "error: child %d semop failed: %s\n", child_no, strerror(errno));
            _exit(1);
        }

        sleep_ms(delay_ms);
    }

    if (sem_change(semid, SEM_EMPTY, -1) < 0 || sem_change(semid, SEM_MUTEX, -1) < 0) {
        fprintf(stderr, "error: child %d final semop failed: %s\n", child_no, strerror(errno));
        _exit(1);
    }

    shared->stanza_no = 0;
    shared->sender_no = child_no;
    shared->finished = 1;
    shared->lines[0][0] = '\0';

    printf("P%d(pid=%ld): finished work\n", child_no, (long)getpid());
    fflush(stdout);

    if (sem_change(semid, SEM_MUTEX, 1) < 0 || sem_change(semid, SEM_FULL, 1) < 0) {
        fprintf(stderr, "error: child %d final semop failed: %s\n", child_no, strerror(errno));
        _exit(1);
    }

    _exit(0);
}

int main(int argc, char **argv) {
    int delay_one = DEFAULT_DELAY_ONE;
    int delay_two = DEFAULT_DELAY_TWO;
    int shmid = -1;
    int semid = -1;
    int finished_children = 0;
    pid_t child1 = -1;
    pid_t child2 = -1;
    struct stanza_message *shared = (void *)-1;
    char result[MAX_STANZAS][LINES_PER_STANZA][LINE_SIZE];
    union semun sem_arg;
    int i;
    int j;

    memset(result, 0, sizeof(result));

    if (argc > 3) {
        fprintf(stderr, "usage: %s [delay_child1_ms] [delay_child2_ms]\n", argv[0]);
        return 1;
    }

    if (argc >= 2 && parse_positive_int(argv[1], &delay_one) != 0) {
        fprintf(stderr, "error: delay_child1_ms must be a positive integer\n");
        return 1;
    }

    if (argc == 3 && parse_positive_int(argv[2], &delay_two) != 0) {
        fprintf(stderr, "error: delay_child2_ms must be a positive integer\n");
        return 1;
    }

    shmid = shmget(IPC_PRIVATE, sizeof(struct stanza_message), IPC_CREAT | 0600);
    if (shmid < 0) {
        fprintf(stderr, "error: shmget failed: %s\n", strerror(errno));
        return 1;
    }

    shared = shmat(shmid, NULL, 0);
    if (shared == (void *)-1) {
        fprintf(stderr, "error: shmat failed: %s\n", strerror(errno));
        shmctl(shmid, IPC_RMID, NULL);
        return 1;
    }
    memset(shared, 0, sizeof(*shared));

    semid = semget(IPC_PRIVATE, 3, IPC_CREAT | 0600);
    if (semid < 0) {
        fprintf(stderr, "error: semget failed: %s\n", strerror(errno));
        cleanup_ipc(shmid, semid, shared);
        return 1;
    }

    sem_arg.val = 1;
    if (semctl(semid, SEM_EMPTY, SETVAL, sem_arg) < 0) {
        fprintf(stderr, "error: semctl(SEM_EMPTY) failed: %s\n", strerror(errno));
        cleanup_ipc(shmid, semid, shared);
        return 1;
    }

    sem_arg.val = 0;
    if (semctl(semid, SEM_FULL, SETVAL, sem_arg) < 0) {
        fprintf(stderr, "error: semctl(SEM_FULL) failed: %s\n", strerror(errno));
        cleanup_ipc(shmid, semid, shared);
        return 1;
    }

    sem_arg.val = 1;
    if (semctl(semid, SEM_MUTEX, SETVAL, sem_arg) < 0) {
        fprintf(stderr, "error: semctl(SEM_MUTEX) failed: %s\n", strerror(errno));
        cleanup_ipc(shmid, semid, shared);
        return 1;
    }

    printf("P0(pid=%ld): created shared memory and semaphores\n", (long)getpid());
    fflush(stdout);

    child1 = fork();
    if (child1 < 0) {
        fprintf(stderr, "error: fork for P1 failed: %s\n", strerror(errno));
        cleanup_ipc(shmid, semid, shared);
        return 1;
    }
    if (child1 == 0) {
        child_work(1, semid, shared, delay_one);
    }

    child2 = fork();
    if (child2 < 0) {
        fprintf(stderr, "error: fork for P2 failed: %s\n", strerror(errno));
        kill(child1, SIGTERM);
        waitpid(child1, NULL, 0);
        cleanup_ipc(shmid, semid, shared);
        return 1;
    }
    if (child2 == 0) {
        child_work(2, semid, shared, delay_two);
    }

    while (finished_children < CHILD_COUNT) {
        if (sem_change(semid, SEM_FULL, -1) < 0 || sem_change(semid, SEM_MUTEX, -1) < 0) {
            fprintf(stderr, "error: parent semop failed: %s\n", strerror(errno));
            kill(child1, SIGTERM);
            kill(child2, SIGTERM);
            waitpid(child1, NULL, 0);
            waitpid(child2, NULL, 0);
            cleanup_ipc(shmid, semid, shared);
            return 1;
        }

        if (shared->finished) {
            finished_children++;
            printf("P0(pid=%ld): child %d reported finish\n", (long)getpid(), shared->sender_no);
        } else if (shared->stanza_no >= 1 && shared->stanza_no <= MAX_STANZAS) {
            int pos = shared->stanza_no - 1;

            for (i = 0; i < LINES_PER_STANZA; i++) {
                snprintf(result[pos][i], LINE_SIZE, "%s", shared->lines[i]);
            }

            printf("P0(pid=%ld): received stanza %d from P%d\n",
                   (long)getpid(),
                   shared->stanza_no,
                   shared->sender_no);
        }
        fflush(stdout);

        shared->finished = 0;
        shared->stanza_no = 0;
        shared->sender_no = 0;
        shared->lines[0][0] = '\0';

        if (sem_change(semid, SEM_MUTEX, 1) < 0 || sem_change(semid, SEM_EMPTY, 1) < 0) {
            fprintf(stderr, "error: parent semop failed: %s\n", strerror(errno));
            kill(child1, SIGTERM);
            kill(child2, SIGTERM);
            waitpid(child1, NULL, 0);
            waitpid(child2, NULL, 0);
            cleanup_ipc(shmid, semid, shared);
            return 1;
        }
    }

    if (waitpid(child1, NULL, 0) < 0 || waitpid(child2, NULL, 0) < 0) {
        fprintf(stderr, "error: waitpid failed: %s\n", strerror(errno));
        cleanup_ipc(shmid, semid, shared);
        return 1;
    }

    printf("\nСкомпонованное стихотворение:\n\n");
    for (i = 0; i < MAX_STANZAS; i++) {
        for (j = 0; j < LINES_PER_STANZA; j++) {
            printf("%s\n", result[i][j]);
        }
        printf("\n");
    }

    if (shmdt(shared) < 0) {
        fprintf(stderr, "error: shmdt failed: %s\n", strerror(errno));
        shmctl(shmid, IPC_RMID, NULL);
        semctl(semid, 0, IPC_RMID);
        return 1;
    }
    shared = (void *)-1;

    if (shmctl(shmid, IPC_RMID, NULL) < 0) {
        fprintf(stderr, "error: shmctl(IPC_RMID) failed: %s\n", strerror(errno));
        semctl(semid, 0, IPC_RMID);
        return 1;
    }

    if (semctl(semid, 0, IPC_RMID) < 0) {
        fprintf(stderr, "error: semctl(IPC_RMID) failed: %s\n", strerror(errno));
        return 1;
    }

    return 0;
}