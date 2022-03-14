from os import path
from time import sleep 
import string
import random

import docker

import aiohttp
import asyncio
import json

from typing import TypedDict, cast

class CIDGeneratorResponse(TypedDict):
    hash: str

class CIDGenerator(object):
    DOCKER_IMAGE_VERSION = "v0p1"
    DOCKER_REDIRECTIONS = {'3000/tcp': ('127.0.0.1', '3030')}
    DOCKER_PORT = 3030
    base_url: str = "http://127.0.0.1:" + str(DOCKER_PORT) + "/"
    session: aiohttp.ClientSession

    client: docker.DockerClient
    # container: docker.Container

    async def __aenter__(self) -> 'CIDGenerator':
        return self

    async def __aexit__(self, exc_type, exc, tb) -> None:
        await self.session.close()

    def __init__(self) -> None:
        client = docker.from_env()
        self.container = client.containers.run("itsmonty/docker-ipfs-only-hash-" + self.DOCKER_IMAGE_VERSION, ports=self.DOCKER_REDIRECTIONS, detach=True)
        print("Starting Docker container")

        # Wait for docker to start
        timeout = 5
        stop_time = 0.5
        elapsed_time = 0
        while self.container.status != 'running' and elapsed_time < timeout:
            sleep(stop_time)
            elapsed_time += stop_time
            self.container.reload()
            continue

        self.session = aiohttp.ClientSession(base_url=self.base_url)
        print("Docker container running")
    
    def __del__(self) -> None:
        self.container.kill()
        self.container.remove()

    async def get_cid_from_file(self, filepath: str) -> str:
        filepath = path.expanduser(filepath)    # Expand '~' into absolute path
        if not path.exists(filepath):
            raise(f"File not found: {filepath}")

        async with aiohttp.ClientSession(base_url=self.base_url) as session:
            hash = None

            boundary = '------' + ''.join(random.choices(string.ascii_lowercase + string.digits, k=20))
            with aiohttp.MultipartWriter('form-data', boundary=boundary) as mpwriter:
                part = mpwriter.append(open(filepath, 'rb'))
                disposition = {'filename': path.basename(filepath), 'name': 'myFile'}
                part.set_content_disposition(disptype='form-data', **disposition)

                try:
                    response = await self.session.post("/api/hashFile", data=mpwriter)
                    response.raise_for_status()
                    hash = cast(CIDGeneratorResponse, await response.json())['hash']
                except aiohttp.ClientError as client_err:
                    print(f"Error getting storage id: {client_err}")
                except Exception as err:
                    print(f"An error ocurred getting storage id: {err}")

                return hash