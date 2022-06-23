# You can find the new timestamped tags here: https://hub.docker.com/r/gitpod/workspace-full/tags
FROM gitpod/workspace-full:2022-05-08-14-31-53

USER root

# Install Hugo from .dev file
# because snap doesn't work in docker and apt-get doesn't allow to lock the version of Hugo
RUN wget -q -O /tmp/hugo_extended.deb https://github.com/gohugoio/hugo/releases/download/v0.101.0/hugo_extended_0.101.0_Linux-64bit.deb \
  && dpkg -i /tmp/hugo_extended.deb \
  && rm /tmp/hugo_extended.deb
