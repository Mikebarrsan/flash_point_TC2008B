# TC2008B Modelación de Sistemas Multiagentes con gráficas computacionales
# Python server to interact with Unity via POST
# Sergio Ruiz-Loza, Ph.D. March 2021

from http.server import BaseHTTPRequestHandler, HTTPServer
import logging
import json

class Server(BaseHTTPRequestHandler):
    
    def _set_response(self):
        self.send_response(200)
        self.send_header('Content-type', 'text/html')
        self.end_headers()
        
    def do_GET(self):
        if (self.path == '/map'):
            response = {
                "walls": [[[2, 0, 0, 2], [2, 0, 0, 0], [2, 'door', 0, 0], [2, 0, 0, 'door'], [2, 2, 0, 0], ['entrance', 0, 0, 2], [2, 0, 0, 0], [2, 2, 0, 0]],
                          [[0, 0, 0, 2], [0, 0, 0, 0], [0, 2, 2, 0], [0, 0, 2, 2], [0, 'door', 2, 0], [0, 0, 2, 'door'], [0, 0, 2, 0], [0, 2, 'door', 0]],
                          [[0, 0, 0, 'entrance'], [0, 'door', 0, 0], [2, 0, 0, 'door'], [2, 0, 0, 0], [2, 0, 0, 0], [2, 2, 0, 0], [2, 0, 0, 2], ['door', 2, 0, 0]],
                          [[0, 0, 2, 2], [0, 2, 2, 0], [0, 0, 2, 2], [0, 0, 'door', 0], [0, 0, 2, 0], [0, 'door', 2, 0], [0, 0, 2, 'door'], [0, 'entrance', 2, 0]],
                          [[2, 0, 0, 2], [2, 0, 0, 0], [2, 0, 0, 0], ['door', 0, 0, 0], [2, 2, 0, 0], [2, 0, 0, 2], [2, 2, 0, 0], [2, 2, 0, 2]],
                          [[0, 0, 2, 2], [0, 0, 2, 0], [0, 0, 'entrance', 0], [0, 0, 2, 0], [0, 'door', 2, 0], [0, 0, 2, 'door'], [0, 'door', 2, 0], [0, 2, 2, 'door']]
                          ],
                "fires": [[0, 0, 0, 0, 0, 0, 0, 0],
                          [0, 2, 2, 0, 0, 0, 0, 0],
                          [0, 2, 2, 2, 2, 0, 0, 0],
                          [0, 0, 0, 2, 0, 0, 0, 0],
                          [0, 0, 0, 0, 0, 2, 2, 0],
                          [0, 0, 0, 0, 0, 2, 0, 0]]
            }

            self._set_response()
            self.wfile.write(str(response).encode('utf-8'))
        elif (self.path == '/step'):
            self._set_response()
            self.wfile.write("Pediste procesar un paso".format(self.path).encode('utf-8'))


def run(server_class=HTTPServer, handler_class=Server, port=8585):
    logging.basicConfig(level=logging.INFO)
    server_address = ('', port)
    httpd = server_class(server_address, handler_class)
    logging.info("Starting httpd...\n") # HTTPD is HTTP Daemon!
    try:
        httpd.serve_forever()
    except KeyboardInterrupt:   # CTRL+C stops the server
        pass
    httpd.server_close()
    logging.info("Stopping httpd...\n")

if __name__ == '__main__':
    from sys import argv
    
    if len(argv) == 2:
        run(port=int(argv[1]))
    else:
        run()



