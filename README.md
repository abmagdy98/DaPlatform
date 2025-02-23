# Da Platform - Container Image Encryption

## Overview

Container Image Encryption is a security-focused project aimed at ensuring the confidentiality of container images stored in registries. The project introduces encryption techniques to protect images at rest and securely manage encryption keys.

## Features

- **Image Encryption at Rest:** Encrypts container images before pushing them to a registry.
- **Secure Key Management:** Uses asymmetric encryption for managing encryption keys.
- **Efficient Access Control:** Only authorized users can decrypt and use encrypted images.
- **Docker Integration:** Works seamlessly with Docker, Buildah, and OpenSSL.
- **Enhanced Security:** Protects container images from unauthorized access, even if the registry is compromised.

## Installation

To use Container Image Encryption, follow these steps:

1. **Install Docker & Buildah:**

   ```bash
   sudo apt install docker.io buildah
   ```

2. **Install OpenSSL:**

   ```bash
   sudo apt install openssl
   ```

3. **Generate RSA Key Pair:**

   ```bash
   openssl genrsa -out private.pem 10240
   openssl rsa -in private.pem -outform PEM -pubout -out public.pem
   ```

## Usage

### Encrypt & Push an Image

```bash
buildah push --encryption-key public.pem image_name docker://registry_url
```

### Pull & Decrypt an Image

```bash
buildah pull --decryption-key private.pem docker://registry_url
```

## System Architecture

The system is designed as a **web platform** that manages encrypted container images. Each image and user has a unique **public-private key pair**, ensuring secure access control.

- **Admins** manage user access and encryption keys.
- **Users** encrypt their images before pushing and decrypt them after pulling.

## Encryption Process

1. **User generates an RSA key pair.**
2. **Before pushing, the image is encrypted** using the public key.
3. **Image is pushed to the registry** in an encrypted format.
4. **To pull, the user provides their private key** for decryption.

## Technologies Used

- **Docker** – Containerization platform
- **Buildah** – CLI tool for building and pushing images
- **OpenSSL** – Cryptographic encryption tool
- **.NET Framework** – Backend development

## Future Work

- Upgrade framework from .NET Framework 4 to **.NET 6** for better performance and security.
- Improve **UI/UX** for a more seamless experience.
- Implement **secure communication channels** for image transfer.

## Contribution

We welcome contributions! To contribute:

1. **Fork the repository**
2. **Create a feature branch** (`feature-new-feature`)
3. **Commit your changes**
4. **Submit a pull request**

## License

This project is licensed under the **MIT License**.

## References

- J. Turnbull, *The Docker Book: Containerization Is the New Virtualization*, 2014.
- A. Mouat, *Using Docker: Developing and Deploying Software with Containers*, 2015.
- L. Rice, *Container Security: Fundamental Technology Concepts*, 2020.

---

This README provides a comprehensive overview of the project, its installation, usage, and contribution guidelines. Let me know if you want to add any additional details! 🚀

