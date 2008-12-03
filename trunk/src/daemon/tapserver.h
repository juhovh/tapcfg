#ifndef TAPSERVER_H
#define TAPSERVER_H

#include "tapcfg.h"

typedef struct tapserver_s tapserver_t;

tapserver_t *tapserver_init(tapcfg_t *tapcfg, int waitms);
void tapserver_destroy(tapserver_t *server);
int tapserver_add_client(tapserver_t *server, int fd);
int tapserver_start(tapserver_t *server, unsigned short port, int listen);
void tapserver_stop(tapserver_t *server);


#endif