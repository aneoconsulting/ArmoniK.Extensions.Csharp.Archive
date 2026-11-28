ARG WORKER_DLL_IMAGE=dockerhubaneo/armonik_worker_dll:0.19.9

FROM --platform=$BUILDPLATFORM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG FRAMEWORK=net8.0
WORKDIR /src
COPY . .


WORKDIR "/src/Tests/ArmoniK.EndToEndTests/ArmoniK.EndToEndTests.Worker"

RUN dotnet publish -a "${TARGETARCH}" --self-contained -c Release -f "${FRAMEWORK}" .


FROM ${WORKER_DLL_IMAGE} AS final

ENV ServiceAssemblyPath=/guest
COPY --from=build /src/Tests/ArmoniK.EndToEndTests/publish/ArmoniK.EndToEndTests.Worker/1.0.0-700/ /guest
