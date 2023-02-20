import torch

if "__main__" == __name__:
    # Check if CUDA is available
    use_cuda = torch.cuda.is_available()
    device = torch.device("cuda" if use_cuda else "cpu")
    print("Using device:", device)

    # Print name of device
    print(torch.cuda.get_device_name(0))
