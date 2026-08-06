import { useEffect } from "react";
import { useNavigate } from "react-router-dom";
import { LoginForm } from "../features/auth";
import { useAuthStore } from "../features/auth/store/useAuthStore";

export const Login = () => {
  const { isAuthenticated } = useAuthStore();
  const navigate = useNavigate();

  useEffect(() => {
    if (isAuthenticated) {
      navigate("/");
    }
  }, [isAuthenticated, navigate]);

  return (
    <LoginForm />
  );
};