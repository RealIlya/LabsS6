#define _POSIX_C_SOURCE 200809L

#include <errno.h>
#include <limits.h>
#include <signal.h>
#include <stdio.h>
#include <stdlib.h>
#include <string.h>
#include <sys/types.h>
#include <sys/wait.h>
#include <time.h>
#include <unistd.h>

#define MIN_N 5
#define MAX_N 15
#define SERIES_TERMS 16
#define LINE_SIZE 256
#define READER_DELAY_MS 100
#define WRITER_DELAY_MS 70

static void sleep_ms(long ms) {
    struct timespec ts;
    ts.tv_sec = ms / 1000;
    ts.tv_nsec = (ms % 1000) * 1000000L;
    nanosleep(&ts, NULL);
}

static double sin_series(double x, int terms) {
    double term = x;
    double sum = x;
    int k;

    for (k = 1; k < terms; k++) {
        term *= -x * x / ((2.0 * k) * (2.0 * k + 1.0));
        sum += term;
    }
    return sum;
}

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

static void child_reader(const char *tmp_path) {
    long pos = 0;
    int done = 0;

    while (!done) {
        FILE *f = fopen(tmp_path, "r");
        if (!f) {
            fprintf(stderr, "error: cannot open '%s' for read: %s\n", tmp_path, strerror(errno));
            _exit(1);
        }

        if (fseek(f, pos, SEEK_SET) != 0) {
            fprintf(stderr, "error: cannot seek '%s'\n", tmp_path);
            fclose(f);
            _exit(1);
        }

        for (;;) {
            char line[LINE_SIZE];
            if (!fgets(line, sizeof(line), f)) {
                break;
            }
            pos = ftell(f);
            if (strcmp(line, "STOP\n") == 0 || strcmp(line, "STOP") == 0) {
                done = 1;
                break;
            }
            printf("reader: %s", line);
            fflush(stdout);
        }

        fclose(f);
        if (!done) {
            sleep_ms(READER_DELAY_MS);
        }
    }
    exit(0);
}

int main(int argc, char **argv) {
    int n = 0;
    int i;
    unsigned int seed;
    pid_t child;
    char tmp_path[256];

    if (argc > 3) {
        fprintf(stderr, "usage: %s [n] [seed]\n", argv[0]);
        return 1;
    }

    if (argc == 3) {
        int parsed_seed = 0;
        if (parse_positive_int(argv[2], &parsed_seed) != 0) {
            fprintf(stderr, "error: seed must be a positive integer\n");
            return 1;
        }
        seed = (unsigned int)parsed_seed;
    } else {
        seed = (unsigned int)time(NULL);
    }

    srand(seed);

    if (argc >= 2) {
        if (parse_positive_int(argv[1], &n) != 0) {
            fprintf(stderr, "error: n must be a positive integer\n");
            return 1;
        }
    } else {
        n = MIN_N + rand() % (MAX_N - MIN_N + 1);
    }

    snprintf(tmp_path, sizeof(tmp_path), "/tmp/urvs_lab2_var2_%ld.tmp", (long)getpid());

    {
        FILE *f = fopen(tmp_path, "w");
        if (!f) {
            fprintf(stderr, "error: cannot create '%s': %s\n", tmp_path, strerror(errno));
            return 1;
        }
        fclose(f);
    }

    child = fork();
    if (child < 0) {
        fprintf(stderr, "error: fork failed: %s\n", strerror(errno));
        unlink(tmp_path);
        return 1;
    }
    if (child == 0) {
        child_reader(tmp_path);
    }

    for (i = 0; i < n; i++) {
        double x = ((double)rand() / (double)RAND_MAX) * 2.0 - 1.0;
        double y = sin_series(x, SERIES_TERMS);
        FILE *f = fopen(tmp_path, "a");
        if (!f) {
            fprintf(stderr, "error: cannot open '%s' for append: %s\n", tmp_path, strerror(errno));
            kill(child, SIGTERM);
            waitpid(child, NULL, 0);
            unlink(tmp_path);
            return 1;
        }
        fprintf(f, "step=%d x=%.8f sin(x)=%.12f\n", i + 1, x, y);
        fclose(f);
        sleep_ms(WRITER_DELAY_MS);
    }

    {
        FILE *f = fopen(tmp_path, "a");
        if (!f) {
            fprintf(stderr, "error: cannot append STOP to '%s': %s\n", tmp_path, strerror(errno));
            kill(child, SIGTERM);
            waitpid(child, NULL, 0);
            unlink(tmp_path);
            return 1;
        }
        fprintf(f, "STOP\n");
        fclose(f);
    }

    if (waitpid(child, NULL, 0) < 0) {
        fprintf(stderr, "error: waitpid failed: %s\n", strerror(errno));
        unlink(tmp_path);
        return 1;
    }

    if (unlink(tmp_path) != 0) {
        fprintf(stderr, "error: cannot remove '%s': %s\n", tmp_path, strerror(errno));
        return 1;
    }

    printf("done: n=%d seed=%u\n", n, seed);
    return 0;
}
