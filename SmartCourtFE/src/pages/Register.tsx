import { useEffect } from "react";
import { useNavigate } from "react-router-dom";
import { RegisterForm } from "../features/auth/components/RegisterForm";
import { useAuthStore } from "../features/auth/store/useAuthStore";

export const Register = () => {
  const { isAuthenticated } = useAuthStore();
  const navigate = useNavigate();

  useEffect(() => {
    if (isAuthenticated) {
      navigate("/");
    }
  }, [isAuthenticated, navigate]);

  return (
    <RegisterForm />
  );
};